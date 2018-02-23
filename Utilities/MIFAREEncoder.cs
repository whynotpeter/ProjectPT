//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Subsembly.SmartCard;
//using Subsembly.SmartCard.PcSc;

//namespace Utilities
//{
//    internal enum MifareKeyType
//    {
//        KeyA,
//        KeyB
//    } ;

//    class MIFAREEncoder:IEncoder
//    {
//        public ByteArray ReadBlock(byte sector, byte block, MifareKeyType keyType, ByteArray key)
//        {
//            byte keyNumber = sector;
//            if (keyType == MifareKeyType.KeyB)
//                keyNumber = (byte)(sector | 0x10);
//            ByteArray data = new ByteArray(16);
//            LoadKey(sector, keyType, key);
//            Byte blockNumber = (Byte)((sector * 0x04) + block);
//            Authenticate(blockNumber, keyType, keyNumber);

//            //read
//            ByteArray apdu = new ByteArray("FF B0 00");
//            apdu += blockNumber;
//            apdu += 0x10;
//            data = SendCommand(apdu);

//            return data;
//        }

//        public void WriteBlock(byte sector, byte block, MifareKeyType keyType, ByteArray key, ByteArray data)
//        {
//            byte keyNumber = sector;
//            if (keyType == MifareKeyType.KeyB)
//                keyNumber = (byte)(sector | 0x10);
//            LoadKey(sector, keyType, key);

//            Byte blockNumber = (Byte)((sector * 0x04) + block);
//            Authenticate(blockNumber, keyType, keyNumber);

//            //write
//            ByteArray apdu = new ByteArray("FF D6 00");
//            apdu += blockNumber;
//            apdu += (byte)data.Length;
//            apdu += data;
//            SendCommand(apdu);
//        }

//        public void LoadKey(byte sector, MifareKeyType keyType, ByteArray key)
//        {
//            byte keyNumber = sector;
//            if (keyType == MifareKeyType.KeyB)
//                keyNumber = (byte)(sector | 0x10);
//            ByteArray apdu = new ByteArray("FF 82 20");
//            apdu += keyNumber;
//            apdu += (byte)0x06;
//            apdu += key;
//            SendCommand(apdu);
//        }
		
//        /// <summary>
//        /// Ustawia prawa dostępu dla wybranego sektora
//        /// </summary>
//        /// <param name="blanket"></param>
//        /// <param name="sector"></param>
//        /// <param name="authKeyType"></param>
//        /// <param name="authKey"></param>
//        /// <param name="keyA"></param>
//        /// <param name="accessConditions">Prawa dostępu kolejno dla bloków 0, 1, 2, trailer</param>
//        /// <param name="keyB"></param>
//        public void DefineAccessConditions(byte sector, MifareKeyType authKeyType, ByteArray authKey, ByteArray keyA, ByteArray accessConditions, ByteArray keyB)
//        {
//            byte keyNumber = sector;
//            if (authKeyType == MifareKeyType.KeyB)
//                keyNumber = (byte)(sector | 0x10);
//            LoadKey(sector, authKeyType, authKey);
//            Byte blockNumber = (Byte)((sector * 0x04) + 3);
//            Authenticate(blockNumber, authKeyType, keyNumber);

//            //prepare access conditions
//            accessConditions = PrepareAccessConditions(accessConditions);

//            //write
//            ByteArray apdu = new ByteArray("FF D6 00");
//            apdu += blockNumber;
//            ByteArray data = keyA + accessConditions + keyB;
//            apdu += (byte)data.Length;
//            apdu += data;
//            SendCommand(apdu);
//        }
		
//        /// <summary>
//        /// Przygotowuje prawa dostępu do zapisania na karcie
//        /// </summary>
//        /// <param name="accessConditions">Prawa dostępu dla bloków 0, 1, 2, trailer</param>
//        /// <returns>Bajty określające prawa dostępu, które należy wysłać do karty</returns>
//        public ByteArray PrepareAccessConditions(ByteArray accessConditions)
//        {
//            BitArray C0 = new BitArray(new byte[] { accessConditions[0] });
//            BitArray C1 = new BitArray(new byte[] { accessConditions[1] });
//            BitArray C2 = new BitArray(new byte[] { accessConditions[2] });
//            BitArray C3 = new BitArray(new byte[] { accessConditions[3] });

//            BitArray byte6 = new BitArray(new bool[] { !C3.Get(6), !C2.Get(6), !C1.Get(6), !C0.Get(6), !C3.Get(5), !C2.Get(5), !C1.Get(5), !C0.Get(5) });
//            BitArray byte7 = new BitArray(new bool[] { C3.Get(5), C2.Get(5), C1.Get(5), C0.Get(5), !C3.Get(7), !C2.Get(7), !C1.Get(7), !C0.Get(7) });
//            BitArray byte8 = new BitArray(new bool[] { C3.Get(7), C2.Get(7), C1.Get(7), C0.Get(7), C3.Get(6), C2.Get(6), C1.Get(6), C0.Get(6) });

//            return new ByteArray(new byte[] { byte6.GetBytes()[0], byte7.GetBytes()[0], byte8.GetBytes()[0], 0x00 });
//        }

//        public ByteArray SendCommand(ByteArray command)
//        {
//            if (!Connected) throw new Exception("Błąd połączenia z kartą");
//            CardCommandAPDU apdu = null;
//            if (command.Length == 4)
//                apdu = new CardCommandAPDU(command[0], command[1], command[2], command[3]);
//            else if (command.Length == 5)
//                apdu = new CardCommandAPDU(command[0], command[1], command[2], command[3], command[4]);
//            else if (command.Length > 5)
//            {
//                ByteArray data = command.Extract(5, command[4]);
//                int length = 5 + command[4];
//                if (length == command.Length)
//                    apdu = new CardCommandAPDU(command[0], command[1], command[2], command[3], data.ByteData);
//                else
//                    apdu = new CardCommandAPDU(command[0], command[1], command[2], command[3], data.ByteData, command[command.Length - 1]);
//            }

//            if (apdu == null)
//                throw new Exception("Nieprawidłowa komenda APDU: " + command);

//            int tryCounter = 5;
//            CardResponseAPDU response;
//            do
//            {
//                try
//                {
//                    response = ((CardExpress)Manager.Instance.PrintingProcess.Printer.cardConnection.card).SendCommand(apdu);
//                    break;
//                }
//                catch (SCardException scException)
//                {
//                    tryCounter--;
//                    if (tryCounter == 0)
//                        throw new Exception("Sprzętowy błąd wykonywania komendy APDU (Kod: " + scException.ResponseCode + ").", scException);
//                }
//                catch (Exception terminalException)
//                {
//                    tryCounter--;
//                    if (tryCounter == 0)
//                        throw new Exception("Sprzętowy błąd wykonywania komendy APDU.", terminalException);
//                }
//            } while (true);

//            ByteArray result = new ByteArray(response.GenerateBytes());

//            if (!response.IsSuccessful)
//                throw new Exception("Błąd wykonywania komendy APDU.", null);

//            return result;
//        }

//        public ByteArray Authenticate(byte block, MifareKeyType keyType, byte keyNumber)
//        {
//            ByteArray apdu = new ByteArray("FF 86 00 00 05 01 00");
//            apdu += block;
//            if (keyType == MifareKeyType.KeyA)
//                apdu += (byte)0x60;
//            else
//                apdu += (byte)0x61;
//            apdu += keyNumber;
//            return SendCommand(apdu);
//        }

//        #region Implementation of IEncoder

//        public ByteArray SendCommand(ByteArray command, bool checkSuccessful)
//        {
//            throw new NotImplementedException();
//        }

//        public bool Ready
//        {
//            get { throw new NotImplementedException(); }
//        }

//        #endregion
//    }
//}
