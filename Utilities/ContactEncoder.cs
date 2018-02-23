using System;
using Subsembly.SmartCard;
using Subsembly.SmartCard.PcSc;
using System.Collections;
using System.Threading;

namespace Utilities
{
    public class ContactEncoder: IEncoder
    {
        /// <summary>
        /// Wybiera terminal
        /// </summary>
        /// <param name="namePattern">Nazwa terminala lub wzorzec nazwy</param>
        public void Initialize(string namePattern)
        {
            TerminalName = null;
            ArrayList terminalNames = CardTerminalManager.Singleton.AllTerminalNames;
            foreach (String terminalName in terminalNames)
            {
                if (!terminalName.Contains(namePattern))
                    continue;

                TerminalName = terminalName;
                break;
            }

            if (TerminalName == null)
                throw new Exception(String.Format("Nie znaleziono terminala o nazwie pasującej do wzorca: {0}", namePattern));

            //SELSCommon.Instance.Logger.Log(String.Format("Monitoruję terminal: {0}", TerminalName));
        }

        public CardHandle CardHandle { get; set; }

        /// <summary>
        /// Nazwa terminala
        /// </summary>
        public string TerminalName { get; set; }

        /// <summary>
        /// Łączy z kartą
        /// </summary>
        /// <param name="force">Wymuszenie zresetowania karty</param>
        /// <returns>Nr ATR karty</returns>
        public ByteArray ConnectCard(bool force = false)
        {
            if (CardHandle != null)
            {
                if (CardHandle.IsValid && !force)
                {
                    return new ByteArray(CardHandle.ATR);
                }
                else
                {
                    CardHandle.Dispose();
                    CardHandle = null;
                }
            }

            int tryCounter = 5;

            do
            {
                try
                {
                    CardHandle = CardTerminalManager.Singleton.AcquireCard(TerminalName);
                }
                catch (System.Runtime.InteropServices.COMException comException)
                {
                    CardHandle = null;
                    tryCounter--;
                    if (tryCounter == 0)
                        throw new Exception("Sprzętowy błąd wykonywania komendy APDU (Message: " + comException.Message + ", HResult: " + comException.ErrorCode + ")");
                }
                catch (SCardException scException)
                {
                    CardHandle = null;
                    tryCounter--;
                    if (tryCounter == 0)
                        throw new Exception("Sprzętowy błąd wykonywania komendy APDU (Kod: " + scException.ResponseCode + ").", scException);
                }
                catch (Exception terminalException)
                {
                    CardHandle = null;
                    tryCounter--;
                    if (tryCounter == 0)
                        throw new Exception("Sprzętowy błąd wykonywania komendy APDU.", terminalException);
                }
                finally
                {
                    if (CardHandle == null)
                    {
                        tryCounter--;
                        if (tryCounter == 0)
                            throw new Exception("Nie znaleziono karty w czytniku.");
                    }
                }
                if (CardHandle != null)
                {
                    return new ByteArray(CardHandle.ATR);
                }
                Thread.Sleep(1000);
            } while (true);
        }

        /// <summary>
        /// Wysyła do karty polecenie APDU
        /// </summary>
        /// <param name="command">Polecenie APDU</param>
        /// <param name="checkSuccessful">Ma sprawdzać, czy odpowiedź jest poprawna (90 00)</param>
        /// <exception cref="APDUException"></exception>
        /// <returns>Odpowiedź na polecenie APDU</returns>
        public ByteArray SendCommand(ByteArray command, bool checkSuccessful = true)
        {
            ConnectCard(false);

            CardCommandAPDU apdu = null;
            if (command.Length == 4)
                apdu = new CardCommandAPDU(command[0], command[1], command[2], command[3]);
            else if (command.Length == 5)
                apdu = new CardCommandAPDU(command[0], command[1], command[2], command[3], command[4]);
            else if (command.Length > 5)
            {
                ByteArray data = command.Extract(5, command[4]);
                int length = 5 + command[4];
                if (length == command.Length)
                    apdu = new CardCommandAPDU(command[0], command[1], command[2], command[3], data.ByteData);
                else
                    apdu = new CardCommandAPDU(command[0], command[1], command[2], command[3], data.ByteData, command[command.Length - 1]);
            }

            if (apdu == null)
                throw new Exception("Nieprawidłowa komenda APDU: " + command);

            int tryCounter = 5;
            CardResponseAPDU response = null;
            do
            {
                try
                {
                    Logger.Log("[Encoder] -> " + command);
                    response = CardHandle.SendCommand(apdu);
                    break;
                }
                catch (System.Runtime.InteropServices.COMException comException)
                {
                    tryCounter--;
                    if (tryCounter == 0)
                        throw new Exception("Sprzętowy błąd wykonywania komendy APDU (Message: " + comException.Message + ", HResult: " + comException.ErrorCode + ")");
                }
                catch (SCardException scException)
                {
                    tryCounter--;
                    if (tryCounter == 0)
                        throw new Exception("Sprzętowy błąd wykonywania komendy APDU (Kod: " + scException.ResponseCode + ").", scException);
                }
                catch (Exception terminalException)
                {
                    tryCounter--;
                    if (tryCounter == 0)
                        throw new Exception("Sprzętowy błąd wykonywania komendy APDU.", terminalException);
                }
            } while (true);

            ByteArray result = new ByteArray(response.GenerateBytes());
            Logger.Log("[Encoder] <- " + result);

            if (checkSuccessful && !response.IsSuccessful && !response.IsWarning)
                throw new APDUException("Błąd wykonywania komendy APDU", command, new ByteArray(response.GenerateBytes()), response.SW1, response.SW2);

            return result;
        }

        public bool Ready { get; set; }
    }
}