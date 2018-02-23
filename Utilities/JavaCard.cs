using System;
using System.IO;
using System.Security.Cryptography;

namespace Utilities
{
    public enum SecurityControlMode { NoSecurity = 0, MAC = 1, MACAndEncryption = 3 };
    public enum DAPType { None, General, Mandated };
    public enum KeyAlgorithm { ECB = 0x81, CBC = 0x82, ECB_CBC = 0x80, AES = 0x88 };
    public enum SecureChannelProtocol { Unknown = 0x00, SCP01 = 0x01, SCP02 = 0x02 };
    public enum KeyDiversificationMethod { None, GPICSN, TagCF, MPCOS3DES }

    public class JavaCard
    {
        public IEncoder Encoder;
        public JavaCardKeys Keys;
        public ByteArray LastInitUpdateResponse;

        private static Random random = new Random();
        private JavaCardKeys _sessionKeys;
        private SecurityControlMode _securityMode;
        private ByteArray _lastMAC;
        private byte _authenticatedKeySetVersion;
        private SecureChannelProtocol scp = SecureChannelProtocol.Unknown;

        #region public

        public JavaCard(IEncoder encoder)
        {
            this.Encoder = encoder;
        }

        public ByteArray SendCommand(ByteArray apdu, bool checkSuccessful = true)
        {
            return Encoder.SendCommand(apdu, checkSuccessful);
        }

        /// <summary>
        /// Dywersyfikuje klucze CM metodą TagCF
        /// </summary>
        /// <remarks>
        /// Korzysta z połączenia z kartą - wysyła Get Data do CM (CM musi być wcześniej wybrany)
        /// </remarks>
        /// <param name="motherKey">Klucz matka do dywersyfikacji</param>
        /// <param name="initUpdateResponse"> </param>
        /// <returns>Klucze Auth, Sign, KEK</returns>
        public JavaCardKeys TagCFDiversificate(ByteArray motherKey, ByteArray initUpdateResponse)
        {
            ByteArray diversificationData = initUpdateResponse.Extract(4, 6);

            ByteArray[] keys = new ByteArray[3];
            ByteArray authEncDivData = diversificationData + new ByteArray("f0 01") + diversificationData + new ByteArray("0f 01");
            keys[0] = authEncDivData.EncodeAsData(motherKey, new ByteArray(8), PaddingMode.None, CipherMode.ECB);
            ByteArray sigDivData = diversificationData + new ByteArray("f0 02") + diversificationData + new ByteArray("0f 02");
            keys[1] = sigDivData.EncodeAsData(motherKey, new ByteArray(8), PaddingMode.None, CipherMode.ECB);
            ByteArray kekDivData = diversificationData + new ByteArray("f0 03") + diversificationData + new ByteArray("0f 03");
            keys[2] = kekDivData.EncodeAsData(motherKey, new ByteArray(8), PaddingMode.None, CipherMode.ECB);
            return new JavaCardKeys(keys);
        }

        /// <summary>
        /// Dywersyfikuje klucze CM metodą GPIC_Serial
        /// </summary>
        /// <param name="motherKey">Klucz matka do dywersyfikacji</param>
        /// <param name="initUpdateResponse"> </param>
        /// <returns>Klucze Auth, Sign, KEK</returns>
        public JavaCardKeys GPICSerialDiversificate(ByteArray motherKey, ByteArray initUpdateResponse)
        {
            ByteArray diversificationData = initUpdateResponse.Extract(0, 2) + initUpdateResponse.Extract(4, 4);

            ByteArray[] keys = new ByteArray[3];
            ByteArray authEncDivData = diversificationData + new ByteArray("f0 01") + diversificationData + new ByteArray("0f 01");
            keys[0] = authEncDivData.EncodeAsData(motherKey, new ByteArray(8), PaddingMode.None, CipherMode.ECB);
            ByteArray sigDivData = diversificationData + new ByteArray("f0 02") + diversificationData + new ByteArray("0f 02");
            keys[1] = sigDivData.EncodeAsData(motherKey, new ByteArray(8), PaddingMode.None, CipherMode.ECB);
            ByteArray kekDivData = diversificationData + new ByteArray("f0 03") + diversificationData + new ByteArray("0f 03");
            keys[2] = kekDivData.EncodeAsData(motherKey, new ByteArray(8), PaddingMode.None, CipherMode.ECB);
            return new JavaCardKeys(keys);
        }

        /// <summary>
        /// Uwierzytelnia do CM podanym kluczem
        /// </summary>
        public JavaCardKeys StartSecuredChannel(ByteArray motherKey, KeyDiversificationMethod diversification = KeyDiversificationMethod.None, SecurityControlMode securityMode = SecurityControlMode.NoSecurity, byte keyVersion = 0x00, byte referenceControlParameter2 = 0x00)
        {
            try
            {
                //losowanie danych terminala
                ByteArray terminalRandom = new ByteArray(8);
                terminalRandom.Randomize(random);

                //Initialize Update
                ByteArray response = InitializeUpdate(keyVersion, 00, terminalRandom);

                //z odpowiedzi wyciągamy dane
                byte scpId = response[11];
                if (scpId == 0x01) 
                    scp = SecureChannelProtocol.SCP01;
                else if (scpId == 0x02) 
                    scp = SecureChannelProtocol.SCP02;
                ByteArray sn = response.Extract(4, 4);
                ByteArray aid = response.Extract(0, 2);
                ByteArray cardRandom = response.Extract(12, 8);
                ByteArray cardCryptogram = response.Extract(20, 8);
                
                Logger.Log("[JavaCard] \n{0}:\t{1}\n{2}:\t{3}\n{4}:\t{5}\n{6}:\t{7}\n{8}:\t{9}", "TerminalRandom", terminalRandom, "SN", sn, "AID", aid, "CardRandom", cardRandom, "CardCryptogram", cardCryptogram);
                
                //wyliczanie kluczy z matki
                switch (diversification)
                {
                    case KeyDiversificationMethod.None:
                        Keys = new JavaCardKeys(motherKey);
                        break;
                    case KeyDiversificationMethod.GPICSN:
                        Keys = GPICSerialDiversificate(motherKey, response);
                        break;
                    case KeyDiversificationMethod.MPCOS3DES:
                        throw new Exception("Wybrany algorytm dywersyfikacji nie jest na razie wspierany");
                        break;
                    case KeyDiversificationMethod.TagCF:
                        Keys = TagCFDiversificate(motherKey, response);
                        break;
                }

                //wyliczamy klucze sesyjne AUTH/ENC i MAC
                if (scp == SecureChannelProtocol.SCP01)
                    ComputeSessionKeys(Keys, terminalRandom, cardRandom);
                else if (scp == SecureChannelProtocol.SCP02)
                    ComputeSessionKeys2(Keys, terminalRandom, cardRandom);
                else
                    throw new Exception("Nieobsługiwany protokół bezpiecznego kanału");

                //weryfikujemy kryptogram zwrócony przez InitializeUpdate
                if (!CheckCardCryptogram(cardCryptogram, terminalRandom, cardRandom))
                    throw new JavaCardAuthenticationException("Kryptogram zwrócony przez komendę InitializeUpdate nie zgadza się.");

                //ExternalAuthenticate
                ByteArray externalAuthenticateCommand = new ByteArray("84 82") + (byte)securityMode + new ByteArray("00 10");
                ByteArray terminalCryptogram = GenerateTerminalCryptogram(terminalRandom, cardRandom);
                ByteArray MAC = new ByteArray();
                if (scp == SecureChannelProtocol.SCP01)
                    MAC = GenerateExAuthMAC(externalAuthenticateCommand, terminalCryptogram);
                else if (scp == SecureChannelProtocol.SCP02)
                    MAC = GenerateExAuthMAC2(externalAuthenticateCommand, terminalCryptogram);
                ByteArray fullExternalAuthenticateCommand = externalAuthenticateCommand + terminalCryptogram + MAC;
                Encoder.SendCommand(fullExternalAuthenticateCommand);
                
                _securityMode = securityMode;
                _lastMAC = MAC;
                _authenticatedKeySetVersion = response[10];
            }
            catch (APDUException exception)
            {
                _sessionKeys = null;
                _securityMode = SecurityControlMode.NoSecurity;
                throw new JavaCardAuthenticationException("Błąd ustanowienia bezpiecznego kanału.", exception);
            }
            catch (Exception exception)
            {
                _sessionKeys = null;
                _securityMode = SecurityControlMode.NoSecurity;
                throw new JavaCardAuthenticationException("Błąd ustanowienia bezpiecznego kanału.", exception);
            }

            return _sessionKeys;
        }

        public ByteArray InitializeUpdate(byte keyVersion = 0x00, byte p2 = 0x00, ByteArray terminalRandom = null)
        {
            if (terminalRandom == null)
                terminalRandom = new ByteArray("00 00 00 00 00 00 00 00");

            ByteArray initializeUpdateCommand = new ByteArray("80 50 00 00 08") + terminalRandom + new ByteArray("00");
            initializeUpdateCommand[2] = keyVersion;
            initializeUpdateCommand[3] = p2;
            ByteArray response = Encoder.SendCommand(initializeUpdateCommand);
            LastInitUpdateResponse = response;
            return response;
        }

        /// <summary>
        /// Uwierzytelnia do CM podanymi kluczami
        /// </summary>
        /// <param name="keys">Klucze</param>
        /// <param name="keySet"></param>
        /// <param name="keyIndex"></param>
        /// <param name="securityMode"></param>
        public JavaCardKeys StartSecuredChannel(JavaCardKeys keys, SecurityControlMode securityMode, Byte keySet = 0, Byte keyIndex = 0)
        {
            try
            {
                //losowanie danych terminala
                ByteArray terminalRandom = new ByteArray(8);
                terminalRandom.Randomize(random);
                //terminalRandom = new ByteArray("00 00 00 00 00 00 00 00");

                //Initialize Update
                ByteArray initializeUpdateCommand = new ByteArray("80 50 00 00 08") + terminalRandom + new ByteArray("00");
                initializeUpdateCommand[2] = keySet;
                initializeUpdateCommand[3] = keyIndex;

                ByteArray response = Encoder.SendCommand(initializeUpdateCommand);

                //z odpowiedzi wyciągamy losowe dane karty i kryptogram
                ByteArray cardRandom = response.Extract(12, 8);
                ByteArray cardCryptogram = response.Extract(20, 8);

                byte scpId = response[11];
                if (scpId == 0x01)
                    scp = SecureChannelProtocol.SCP01;
                else if (scpId == 0x02)
                    scp = SecureChannelProtocol.SCP02;

                //wyliczamy klucze sesyjne AUTH/ENC i MAC
                if (scp == SecureChannelProtocol.SCP01)
                    ComputeSessionKeys(Keys, terminalRandom, cardRandom);
                else if (scp == SecureChannelProtocol.SCP02)
                    ComputeSessionKeys2(Keys, terminalRandom, cardRandom);
                else
                    throw new Exception("Nieobsługiwany protokół bezpiecznego kanału");

                //weryfikujemy kryptogram zwrócony przez InitializeUpdate
                if (!CheckCardCryptogram(cardCryptogram, terminalRandom, cardRandom))
                    throw new JavaCardAuthenticationException("Kryptogram zwrócony przez komendę InitializeUpdate nie zgadza się.");

                //ExternalAuthenticate
                ByteArray externalAuthenticateCommand = new ByteArray("84 82") + (byte)securityMode + new ByteArray("00 10");
                ByteArray terminalCryptogram = GenerateTerminalCryptogram(terminalRandom, cardRandom);
                ByteArray MAC = GenerateExAuthMAC(externalAuthenticateCommand, terminalCryptogram);

                ByteArray fullExternalAuthenticateCommand = externalAuthenticateCommand + terminalCryptogram + MAC;
                Encoder.SendCommand(fullExternalAuthenticateCommand);
                _securityMode = securityMode;
                _lastMAC = MAC;
                _authenticatedKeySetVersion = response[10];
            }
            catch (APDUException exception)
            {
                _sessionKeys = null;
                _securityMode = SecurityControlMode.NoSecurity;
                throw new JavaCardAuthenticationException("Błąd ustanowienia bezpiecznego kanału.", exception);
            }
            catch (Exception exception)
            {
                _sessionKeys = null;
                _securityMode = SecurityControlMode.NoSecurity;
                throw new JavaCardAuthenticationException("Błąd ustanowienia bezpiecznego kanału.", exception);
            }
            return _sessionKeys;
        }

        /// <summary>
        /// Wysyła polecenie apdu bezpiecznym kanałem
        /// </summary>
        /// <param name="command">apdu</param>
        /// <returns></returns>
        public ByteArray SendSecuredCommand(ByteArray command)
        {
            byte CLA, INS, P1, P2, Lc, Le;
            Boolean hasLe = false;
            ByteArray data = new ByteArray();
            Logger.Log("[JavaCard] +> " + command);

            //rozbijamy APDU na części składowe
            CLA = command[0];
            INS = command[1];
            P1 = command[2];
            P2 = command[3];
            Lc = 0x00;
            Le = 0x00;
            if (command.Length == 5)
            {
                Le = command[4];
                hasLe = true;
            }
            else if (command.Length > 5)
            {
                Lc = command[4];
                data = command.Extract(5, Lc);
                if (command.Length > Lc + 5)
                {
                    Le = command[command.Length - 1];
                    hasLe = true;
                }
            }

            ByteArray commandToSend = new ByteArray(command.ByteData);

            if (_securityMode == SecurityControlMode.MAC)
            {
                ByteArray toMac = new ByteArray(new byte[] { (byte)(CLA | 0x04), INS, P1, P2, (byte)(Lc + 8) }) + data; //Lc zwiększamy o 8 - długość MACa
                commandToSend = new ByteArray(toMac.StringData);

                //macujemy
                ByteArray macData = MacData(toMac);
                _lastMAC = macData.LSB(8);
                commandToSend += _lastMAC;
                
                if (hasLe)
                    commandToSend += Le;
            }
            else if (_securityMode == SecurityControlMode.MACAndEncryption)
            {
                //szyfrujemy
                ByteArray toEncrypt = data;
                ByteArray encryptedData = EncryptData(toEncrypt);

                //macujemy
                ByteArray toMac = new ByteArray(new byte[] { (byte)(CLA | 0x04), INS, P1, P2, (byte)(Lc + 8) }) + data;
                ByteArray macData = MacData(toMac);
                _lastMAC = macData.LSB(8);

                commandToSend = new ByteArray(new byte[] { (byte)(CLA | 0x04), INS, P1, P2, (byte) (encryptedData.Length + _lastMAC.Length)}) + encryptedData + _lastMAC;
                if (hasLe)
                    commandToSend += Le;
            }

            return Encoder.SendCommand(commandToSend);
        }

        private ByteArray EncryptData(ByteArray toEncrypt)
        {
            AddPaddingISO9797_2(toEncrypt);

            //szyfrowanie
            ByteArray encryptedData = toEncrypt.EncodeAsData(_sessionKeys.AuthEncKey, new ByteArray(8), PaddingMode.None, CipherMode.ECB);  //było CBC
            return encryptedData;
        }

        private static void AddPaddingISO9797_2(ByteArray toPad)
        {
            //ISO 9797 method 2 padding (0x80 0x00 ...)
            toPad[toPad.Length] = 0x80;
            if (toPad.Length%8 > 0)
                toPad[toPad.Length + (8 - (toPad.Length%8)) - 1] = 0x00;
        }

        private ByteArray MacData(ByteArray toMac)
        {
            AddPaddingISO9797_2(toMac);

            //szyfrujemy
            ByteArray macData = toMac.EncodeAsData(_sessionKeys.SignKey, _lastMAC, PaddingMode.None, CipherMode.CBC);
            return macData;
        }

        /// <summary>
        /// Wysyła polecenie APDU do karty
        /// </summary>
        /// <param name="command">Polecenie APDU</param>
        /// <returns>Odpowiedź karty</returns>
        public ByteArray SendCommand(ByteArray command)
        {
            return Encoder.SendCommand(command);
        }

        /// <summary>
        /// Ładuje plik z pakietem na kartę.
        /// </summary>
        /// <param name="packageContents">Zawartość pliku.</param>
        /// <param name="packageAID">AID pakietu.</param>
        /// <param name="securityDomainAID">AID Security Domain, z którym ma być powiązany pakiet. Może być pustą tablicą, wtedy pakiet zostanie powiązny z CardManagerem.</param>
        public void LoadPackage(ByteArray packageContents, ByteArray packageAID, ByteArray securityDomainAID)
        {
            //install for load
            ByteArray installCommand = new ByteArray("80 e6 02 00");
            ByteArray data = new ByteArray();
            //długość AID pakietu
            data += (byte)packageAID.Length;
            //AID pakietu
            data += packageAID;
            //długość AID security domain (może być CardManagera)
            data += (byte)securityDomainAID.Length;
            //AID security domain (może być CardManagera)
            data += securityDomainAID;

            data += new ByteArray("00 00 00");

            installCommand += (byte)data.Length;
            installCommand += data;
            installCommand += 0x00;

            SendSecuredCommand(installCommand);

            //load
            ByteArray loadCommand = new ByteArray("80 e8 00 00");

            //pierwszy blok, wrzucamy tylko rozmiar pakietu
            UInt16 size = (UInt16)packageContents.Length;
            ByteArray firstBlockLoadCommand = new ByteArray(loadCommand.StringData);
            firstBlockLoadCommand += 0x04;
            firstBlockLoadCommand += 0xC4;
            firstBlockLoadCommand += 0x82;
            firstBlockLoadCommand += new ByteArray(BitConverter.GetBytes(size)).Reversed;
            firstBlockLoadCommand += 0x00;

            SendSecuredCommand(firstBlockLoadCommand);

            //kolejne bloki, już z danymi
            ByteArray[] dataSlices = packageContents.Slice(230, false, 0x00);

            for (Byte blockNumber = 0x00; blockNumber < dataSlices.Length; blockNumber += 1)
            {
                ByteArray blockLoadCommand = new ByteArray(loadCommand.StringData);
                //czy ostatni blok
                if (blockNumber == dataSlices.Length - 1)
                    blockLoadCommand[2] = 0x80;
                //+1 bo pierwszy blok był bez danych
                blockLoadCommand[3] = (byte)(blockNumber + 1);

                blockLoadCommand += (byte)dataSlices[blockNumber].Length;
                blockLoadCommand += dataSlices[blockNumber];
                blockLoadCommand += 0x00;

                SendSecuredCommand(blockLoadCommand);
            }
        }

        /// <summary>
        /// Ładuje plik z pakietem na kartę.
        /// </summary>
        /// <param name="capFileName">Ścieżka i nazwa pliku do zainstalowania.</param>
        /// <param name="packageAID">AID pakietu.</param>
        /// <param name="securityDomainAID">AID Security Domain, z którym ma być powiązany pakiet. Może być pustą tablicą, wtedy pakiet zostanie powiązny z CardManagerem.</param>
        public void LoadPackage(string capFileName, ByteArray packageAID, ByteArray securityDomainAID)
        {
            ByteArray content = ByteArray.LoadFromFile(capFileName);
            LoadPackage(content, packageAID, securityDomainAID);
        }

        /// <summary>
        /// Instaluje applet ze wskazanego pakietu.
        /// </summary>
        /// <param name="packageAID">AID pakietu.</param>
        /// <param name="classAID">AID klasy wewnętrz pakietu.</param>
        /// <param name="instanceAID">AID instancji nowego appletu.</param>
        /// <param name="installParameters">Parametry instalacyjne do przesłania do metody install() appletu.</param>
        /// <param name="options"> </param>
        public void InstallPackage(ByteArray packageAID, ByteArray classAID, ByteArray instanceAID, ByteArray installParameters, AppletInstallOptions options)
        {
            ByteArray installCommand = new ByteArray("80 e6 04 00");
            if (options.selectable)
                installCommand[2] = 0x0c;

            //pełne parametry instalacyjne
            ByteArray completeInstallParameters = new ByteArray();
            //Tag C9
            completeInstallParameters += 0xc9;
            //długość parametrów instalacyjnych appetu
            completeInstallParameters += (byte)installParameters.Length;
            //parametry instalacyjne appletu
            completeInstallParameters += installParameters;
            //systemowe parametry instalacyjne
            if (options.maxMemoryUsage > 0)
            {
                //Tag EF
                completeInstallParameters += 0xef;
                //długość 4 bajty
                completeInstallParameters += 0x04;
                //Tag C8 - maksymalna wielkość EPROM dostępna dla appletu
                completeInstallParameters += 0xc8;
                //długość 2 bajty
                completeInstallParameters += 0x02;
                //wielkość EPROM
                completeInstallParameters += new ByteArray(BitConverter.GetBytes(options.maxMemoryUsage));
            }

            ByteArray data = new ByteArray();
            //długość AID pakietu
            data += (byte)packageAID.Length;
            //AID pakietu
            data += packageAID;
            //długość AID klasy w pakiecie
            data += (byte)classAID.Length;
            //AID klasy w pakiecie
            data += classAID;
            //długość AID instancji
            data += (byte)instanceAID.Length;
            //AID instancji
            data += instanceAID;
            //długość pola z uprawnieniami (musi być 00)
            data += 0x01;
            //uprawnienia
            data += options.privileges.GetAsByte();
            //długość parametrów instalacyjnych appletu
            data += (byte)completeInstallParameters.Length;
            //parametry instalacyjne appletu
            data += completeInstallParameters;
            //długośc install tokena (musi być 00)
            data += 0x00;

            installCommand += (byte)data.Length;
            installCommand += data;
            installCommand += 0x00;

            SendSecuredCommand(installCommand);
        }

        public ByteArray Extradition(ByteArray sdAid, ByteArray instanceAid, ByteArray extraditionToken = null)
        {
            ByteArray dataField = new ByteArray();
            dataField += (byte)sdAid.Length + sdAid; //security domain aid
            dataField += 0x00; //Length = 0 - empty field
            dataField += (byte)instanceAid.Length + instanceAid; //instance aid
            dataField += 0x00; //Length = 0 - empty field
            dataField += 0x00; //Length = 0 - empty field
            if (extraditionToken != null && extraditionToken.Length > 0)
            {
                dataField += (byte)extraditionToken.Length + extraditionToken;
            }
            else
            {
                dataField += 0x00; //Length = 0 - no extradition token
            }

            ByteArray apdu = new ByteArray("80 E6 10 00") + (byte)dataField.Length + dataField + 0x00; //0x00 - Le
            return SendSecuredCommand(apdu);
        }

        /// <summary>
        /// Usuwa applet lub pakiet.
        /// </summary>
        /// <param name="AID">AID appletu lub pakietu.</param>
        public void DeleteObject(ByteArray AID)
        {
            ByteArray deleteCommand = new ByteArray("80 e4 00 00");
            //ByteArray deleteCommand = new ByteArray("80 e4 00 80");

            ByteArray data = new ByteArray();
            data += 0x4f;
            data += (byte)AID.Length;
            data += AID;

            deleteCommand += (byte)data.Length;
            deleteCommand += data;
            deleteCommand += 0x00;

            SendSecuredCommand(deleteCommand);
        }

        /// <summary>
        /// Wywołanie SELECT'a.
        /// </summary>
        /// <remarks>Blankiet musi znajdować się w czytniku przed wywołaniem tej metody.</remarks>
        public ByteArray Select(ByteArray fileID)
        {
            if (fileID.Length > 0)
            {
                ByteArray response = Encoder.SendCommand(new ByteArray("00 a4 04 00") + (byte)fileID.Length + fileID);
                if (response[0] == 0x61)
                    response = Encoder.SendCommand(new ByteArray("00 c0 00 00") + response[1]);
                return response;
            }
            return new ByteArray();
        }

        public void PutKeys(Byte currentKeySetVersion, Byte newKeySetVersion, JavaCardKeys newKeys, KeyAlgorithm cryptAlgorithm, Boolean extraDataByte)
        {
            //komenda PutKey
            ByteArray command = new ByteArray("80 d8 00 81");
            command[2] = currentKeySetVersion;

            //obliczamy pola z kluczami
            ByteArray keyStructures = new ByteArray();

            foreach (ByteArray key in new ByteArray[3] { newKeys.AuthEncKey, newKeys.SignKey, newKeys.KEKKey })
            {
                ByteArray keyStructure = new ByteArray();

                //identyfikator algorytmu
                keyStructure += (byte)cryptAlgorithm;

                //długość klucza
                keyStructure += (Byte)key.Length;

                //klucz zaszyfrowany kluczem KEK
                keyStructure += key.EncodeAsData(_sessionKeys.KEKKey, new ByteArray(8), PaddingMode.None, CipherMode.ECB);

                //długość sumy kontrolnej
                keyStructure += 0x03;

                //check value, 3 najbardziej znaczące bajty z operacji szyfrowania danych w postaci 8 bajtów o wartości 0x00 kluczem do załadowania
                keyStructure += new ByteArray(8).EncodeAsData(key, new ByteArray(8), PaddingMode.None, CipherMode.ECB).MSB(3);

                keyStructures += keyStructure;
            }

            //tworzymy pole z danymi
            ByteArray dataField = new ByteArray();

            //nowa wersja klucza
            dataField += newKeySetVersion;

            //pola z kluczami
            dataField += keyStructures;

            //znacznik końca pola z danymi (opcjonalny)
            if (extraDataByte)
                dataField += 0xff;

            //ustawiamy w komendzie długość pola z danymi (Lc)
            command += (Byte)dataField.Length;

            //dodajemy pole z danymi (Data)
            command += dataField;

            //Le
            //command += 0x0a;
            //command += 0x00;

            //wysyłamy komendę
            SendSecuredCommand(command);
        }

        public void PutKeysStoreData(Byte currentKeySetVersion, Byte newKeySetVersion, JavaCardKeys newKeys, KeyAlgorithm cryptAlgorithm)
        {
            //komenda PutKey
            ByteArray command = new ByteArray("80 e2 88 00");

            ByteArray keysField = new ByteArray();
            ByteArray kcvField = new ByteArray();

            foreach (ByteArray key in new ByteArray[3] { newKeys.AuthEncKey, newKeys.SignKey, newKeys.KEKKey })
            {
                //klucz zaszyfrowany kluczem KEK
                var x = key.EncodeAsData(_sessionKeys.KEKKey, new ByteArray(8), PaddingMode.None, CipherMode.ECB);
                keysField += x;

                //check value, 3 najbardziej znaczące bajty z operacji szyfrowania danych w postaci 8 bajtów o wartości 0x00 kluczem do załadowania
                var kcv = GetKcv(key);
                kcvField += kcv;
            }

            //tworzymy pole z danymi
            ByteArray dataField = new ByteArray();
            dataField += "8F 01 30"; //with length
            dataField += keysField;

            dataField += "7F 01 0C"; //with length
            dataField += currentKeySetVersion;
            dataField += newKeySetVersion;
            dataField += (byte)cryptAlgorithm;
            dataField += kcvField;

            //ustawiamy w komendzie długość pola z danymi (Lc)
            command += (Byte)dataField.Length;

            //dodajemy pole z danymi (Data)
            command += dataField;

            //Le
            //command += 0x0a;
            //command += 0x00;

            //wysyłamy komendę
            SendSecuredCommand(command);
        }

        public static ByteArray GetKcv(ByteArray key)
        {
            var kcv = new ByteArray(8).EncodeAsData(key, new ByteArray(8), PaddingMode.None, CipherMode.ECB).MSB(3);
            return kcv;
        }

        public ByteArray ListApplications()
        {
            // p1 = 80, 40, 20, 10
            // p2 = xxxxxxx0 - first or all, xxxxxxx1 - next, xxxxxx0x - less info, xxxxxx1x - more info
            try
            {
                ByteArray apdu = new ByteArray("80 F2 80 00 02 4F 00 00");
                ByteArray response = SendSecuredCommand(apdu);
                //return response;


                apdu = new ByteArray("80 F2 40 00 02 4F 00 00");
                response = SendSecuredCommand(apdu);
                //return response;



                apdu = new ByteArray("80 F2 20 00 02 4F 00 00");
                response = SendSecuredCommand(apdu);
                apdu = new ByteArray("80 F2 20 01 02 4F 00 00");
                response = response.Extract(0, response.Length - 2);
                response += SendSecuredCommand(apdu);

                return response;




                apdu = new ByteArray("80 F2 10 00 02 4F 00 00");
                response = SendSecuredCommand(apdu);
                return response;
            }
            catch(APDUException exception)
            {
                return null;
            }
        }
        
        #endregion

        #region private

        private void ComputeSessionKeys(JavaCardKeys keys, ByteArray terminalRandom, ByteArray cardRandom)
        {
            //scp01
            ByteArray dataDiversifier = cardRandom.LSB(4) + terminalRandom.MSB(4) + cardRandom.MSB(4) + terminalRandom.LSB(4);
            _sessionKeys = new JavaCardKeys();
            _sessionKeys.AuthEncKey = dataDiversifier.EncodeAsData(keys.AuthEncKey, new ByteArray(8), PaddingMode.None, CipherMode.ECB);
            _sessionKeys.SignKey = dataDiversifier.EncodeAsData(keys.SignKey, new ByteArray(8), PaddingMode.None, CipherMode.ECB);
            _sessionKeys.KEKKey = keys.KEKKey;

            Logger.Log("[JavaCard] Liczenie kluczy sesyjnych\n{0}:\t{1}\n{2}:\t{3}\n{4}:\t{5}", "Auth/Enc", _sessionKeys.AuthEncKey, "Mac", _sessionKeys.SignKey, "Kek", _sessionKeys.KEKKey);
        }

        private void ComputeSessionKeys2(JavaCardKeys keys, ByteArray terminalRandom, ByteArray cardRandom)
        {
            _sessionKeys = new JavaCardKeys();

            ByteArray dataDiversifierAuthEnc = new ByteArray("0182") + cardRandom.MSB(2) + new ByteArray(12);
            _sessionKeys.AuthEncKey = dataDiversifierAuthEnc.EncodeAsData(keys.AuthEncKey, new ByteArray(8), PaddingMode.None, CipherMode.CBC);

            ByteArray dataDiversifierSignC = new ByteArray("0101") + cardRandom.MSB(2) + new ByteArray(12);
            _sessionKeys.SignKeyC = dataDiversifierSignC.EncodeAsData(keys.SignKey, new ByteArray(8), PaddingMode.None, CipherMode.CBC);

            ByteArray dataDiversifierSignR = new ByteArray("0102") + cardRandom.MSB(2) + new ByteArray(12);
            _sessionKeys.SignKeyR = dataDiversifierSignR.EncodeAsData(keys.SignKey, new ByteArray(8), PaddingMode.None, CipherMode.CBC);

            ByteArray dataDiversifierKEK = new ByteArray("0181") + cardRandom.MSB(2) + new ByteArray(12);
            _sessionKeys.KEKKey = dataDiversifierKEK.EncodeAsData(keys.KEKKey, new ByteArray(8), PaddingMode.None, CipherMode.CBC);

            Logger.Log("[JavaCard] Liczenie kluczy sesyjnych\n{0}:\t{1}\n{2}:\t{3}\n{4}:\t{5}\n{6}:\t{7}", "Auth/Enc", _sessionKeys.AuthEncKey, "Mac-C", _sessionKeys.SignKeyC, "Mac-R", _sessionKeys.SignKeyR, "Kek", _sessionKeys.KEKKey);
        }

        private Boolean CheckCardCryptogram(ByteArray cardCryptogram, ByteArray terminalRandom, ByteArray cardRandom)
        {
            //ByteArray input = terminalRandom + cardRandom + new ByteArray("80 00 00 00 00 00 00 00");
            //ByteArray result = input.EncodeAsData(SessionKeys.AuthEncKey, new ByteArray(8), PaddingMode.None, CipherMode.CBC);
            //return cardCryptogram.Equals(result.LSB(8));


            ByteArray padding = new ByteArray("80 00 00 00 00 00 00 00");
            ByteArray key_left = _sessionKeys.AuthEncKey.MSB(8); //MSB mac key
            ByteArray key_right = _sessionKeys.AuthEncKey.LSB(8);
            ByteArray iv = new ByteArray(8, 0x00);

            ByteArray result1 = terminalRandom.SimpleEncodeAsData(key_left, iv, PaddingMode.None, CipherMode.CBC);
            ByteArray result2 = result1.SimpleDecodeAsData(key_right, iv, PaddingMode.None, CipherMode.CBC);
            ByteArray result3 = result2.SimpleEncodeAsData(key_left, iv, PaddingMode.None, CipherMode.CBC);
            ByteArray result4 = result3.XOR(cardRandom);
            ByteArray result5 = result4.SimpleEncodeAsData(key_left, iv, PaddingMode.None, CipherMode.CBC);
            ByteArray result6 = result5.SimpleDecodeAsData(key_right, iv, PaddingMode.None, CipherMode.CBC);
            ByteArray result7 = result6.SimpleEncodeAsData(key_left, iv, PaddingMode.None, CipherMode.CBC);
            ByteArray result8 = result7.XOR(padding);
            ByteArray result9 = result8.SimpleEncodeAsData(key_left, iv, PaddingMode.None, CipherMode.CBC);
            ByteArray result10 = result9.SimpleDecodeAsData(key_right, iv, PaddingMode.None, CipherMode.CBC);
            ByteArray result11 = result10.SimpleEncodeAsData(key_left, iv, PaddingMode.None, CipherMode.CBC);

            Logger.Log("[JavaCard] Wyliczanie terminal Cryptogram (kolejne wyniki algorytmu DES)\n{0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}\n{8}\n{9}\n{10}", result1, result2, result3, result4, result5, result6, result7, result8, result9, result10, result11);

            return cardCryptogram.Equals(result11);
        }

        private ByteArray GenerateTerminalCryptogram(ByteArray terminalRandom, ByteArray cardRandom)
        {
            ByteArray padding = new ByteArray("80 00 00 00 00 00 00 00");

            //ByteArray input = cardRandom + terminalRandom + padding;
            //ByteArray result = input.EncodeAsData(SessionKeys.AuthEncKey, new ByteArray(8, 0x00), PaddingMode.None, CipherMode.CBC);
            //return result.LSB(8);

            ByteArray key_left = _sessionKeys.AuthEncKey.MSB(8); //MSB mac key
            ByteArray key_right = _sessionKeys.AuthEncKey.LSB(8);
            ByteArray iv = new ByteArray(8, 0x00);

            ByteArray result1 = cardRandom.SimpleEncodeAsData(key_left, iv, PaddingMode.None, CipherMode.CBC);
            ByteArray result2 = result1.SimpleDecodeAsData(key_right, iv, PaddingMode.None, CipherMode.CBC);
            ByteArray result3 = result2.SimpleEncodeAsData(key_left, iv, PaddingMode.None, CipherMode.CBC);
            ByteArray result4 = result3.XOR(terminalRandom);
            ByteArray result5 = result4.SimpleEncodeAsData(key_left, iv, PaddingMode.None, CipherMode.CBC);
            ByteArray result6 = result5.SimpleDecodeAsData(key_right, iv, PaddingMode.None, CipherMode.CBC);
            ByteArray result7 = result6.SimpleEncodeAsData(key_left, iv, PaddingMode.None, CipherMode.CBC);
            ByteArray result8 = result7.XOR(padding);
            ByteArray result9 = result8.SimpleEncodeAsData(key_left, iv, PaddingMode.None, CipherMode.CBC);
            ByteArray result10 = result9.SimpleDecodeAsData(key_right, iv, PaddingMode.None, CipherMode.CBC);
            ByteArray result11 = result10.SimpleEncodeAsData(key_left, iv, PaddingMode.None, CipherMode.CBC);

            Logger.Log("[JavaCard] Wyliczanie terminal Cryptogram (kolejne wyniki algorytmu DES)\n{0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}\n{8}\n{9}\n{10}", result1, result2, result3, result4, result5, result6, result7, result8, result9, result10, result11);

            return result11;
        }

        private ByteArray GenerateExAuthMAC(ByteArray command, ByteArray terminalCryptogram)
        {
            ByteArray input = command.Extract(0, 5) + terminalCryptogram + new ByteArray("80 00 00");
            ByteArray result = input.EncodeAsData(_sessionKeys.SignKey, new ByteArray(8), PaddingMode.None, CipherMode.CBC);
            return result.LSB(8);
        }

        private ByteArray GenerateExAuthMAC2(ByteArray command, ByteArray terminalCryptogram)
        {
            ByteArray input = command.Extract(0, 5) + terminalCryptogram + new ByteArray("80 00 00");

            ByteArray mac_key_left = _sessionKeys.SignKey.MSB(8); //MSB mac key
            ByteArray mac_key_right = _sessionKeys.SignKey.LSB(8);
            ByteArray iv = new ByteArray(8, 0x00);

            //ByteArray result = input.EncodeAsData(SessionKeys.SignKey, new ByteArray(8), PaddingMode.None, CipherMode.CBC);
            //ByteArray result = input.EncodeAsData(mac_key_left + mac_key_right, iv, PaddingMode.None, CipherMode.CBC);
            //ByteArray result_comp = result.LSB(8);

            ByteArray apdu_left = input.MSB(8);
            ByteArray apdu_right = input.LSB(8);

            ByteArray result1 = apdu_left.SimpleEncodeAsData(mac_key_left, iv, PaddingMode.None, CipherMode.CBC);
            ByteArray result2 = result1.XOR(apdu_right);
            ByteArray result3 = result2.SimpleEncodeAsData(mac_key_left, iv, PaddingMode.None, CipherMode.CBC);
            ByteArray result4 = result3.SimpleDecodeAsData(mac_key_right, iv, PaddingMode.None, CipherMode.CBC);
            ByteArray result5 = result4.SimpleEncodeAsData(mac_key_left, iv, PaddingMode.None, CipherMode.CBC);

            Logger.Log("[JavaCard] Wyliczanie MAC dla APDU {0} (kolejne wyniki algorytmu DES)\n{1}\n{2}\n{3}\n{4}\n{5}", input, result1, result2, result3, result4, result5);

            return result5;
        }

        #endregion
    }

    public class JavaCardKeys
    {
        public ByteArray AuthEncKey { get; set; }
        public ByteArray SignKey { get { return SignKeyC; } set { SignKeyC = value; } }
        public ByteArray KEKKey { get; set; }
        
        public ByteArray SignKeyC { get; set; }
        public ByteArray SignKeyR { get; set; }

        public JavaCardKeys()
        {
            AuthEncKey = null;
            SignKey = null;
            KEKKey = null;
        }

        public JavaCardKeys(ByteArray allKeysValue)
        {
            AuthEncKey = allKeysValue;
            SignKey = allKeysValue;
            KEKKey = allKeysValue;
        }

        public JavaCardKeys(ByteArray[] keys)
        {
            AuthEncKey = keys[0];
            SignKey = keys[1];
            KEKKey = keys[2];
        }
    }

    public class AppletInstallOptions
    {
        public AppletPrivileges privileges { get; set; }
        public Boolean selectable { get; set; }
        public UInt16 maxMemoryUsage { get; set; }

        public AppletInstallOptions()
        {
            privileges = new AppletPrivileges();
            selectable = true;
            maxMemoryUsage = 0;
        }

        public AppletInstallOptions(AppletPrivileges privileges)
        {
            this.privileges = privileges;
            selectable = true;
            maxMemoryUsage = 0;
        }
    }

    public class AppletPrivileges
    {
        public Boolean isSecurityDomain { get; set; }
        public DAPType DAPVerification { get; set; }
        public Boolean canLockCard { get; set; }
        public Boolean canTerminateCard { get; set; }
        public Boolean canChangePIN { get; set; }
        public Boolean delegatedManagement { get; set; }
        public Boolean defaultSelected { get; set; }

        public AppletPrivileges()
        {
            isSecurityDomain = false;
            DAPVerification = DAPType.None;
            canLockCard = false;
            canTerminateCard = false;
            canChangePIN = false;
            delegatedManagement = false;
            defaultSelected = false;
        }
        
        public Byte GetAsByte()
        {
            Byte result = 0x00;
            if (isSecurityDomain)
                result |= 0x80;
            if (canLockCard)
                result |= 0x10;
            if (canTerminateCard)
                result |= 0x08;
            if (canChangePIN)
                result |= 0x02;
            if (DAPVerification != DAPType.None)
                result |= 0x40;
            if (DAPVerification == DAPType.Mandated)
                result |= 0x01;
            if (delegatedManagement)
                result |= 0x20;
            if (defaultSelected)
                result |= 0x04;

            return result;
        }

        public new string ToString()
        {
            return string.Format("{0:X2}", GetAsByte());
        }
    }
}
