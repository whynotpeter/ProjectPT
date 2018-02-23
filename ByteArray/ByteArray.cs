using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace Utilities
{
    /// <summary>
    /// Pomocnicza klasa ułatwiająca pracę z tablicami bajtów. Dodatkową możliwością w stosunku do
    /// zwykłej tablicy bajtów jest możliwość przypisania i odczytania ciągu znaków zawierającego
    /// cyfry szesnastkowe, sumowanie tablic, wyciąganie podciągów bajtów, szyfrowanie i deszyfrowanie,
    /// XORowanie, wyciąganie bajtów LSB i MSB.
    /// </summary>
    [Serializable]
    public class ByteArray
    {
        private static Random _random = new Random();

        /// <summary>
        /// Wewnętrzna reprezentacja danych - tablica bajtów;
        /// </summary>
        private Byte[] _data;

        /// <summary>
        /// Domyślny separator par bajtów w przypadku wyświetlania obiektu w postaci ciągu znaków.
        /// </summary>
        private String _defaultSeparator;

        /// <summary>
        /// Czy wyświetlane litery mają być wielkie.
        /// </summary>
        private Boolean _upperCase;

        /// <summary>
        /// Przeładowanie operatora [].
        /// </summary>
        /// <param name="i">Indeks.</param>
        /// <returns>Bajt na podanej pozycji.</returns>
        [XmlIgnore]
        public byte this[int i]
        {
            get
            {
                if (i > Length - 1)
                    SetLength(i + 1);
                return _data[i];
            }
            set
            {
                if (i > Length - 1)
                    SetLength(i + 1);
                _data[i] = value;
            }
        }

        /// <summary>
        /// Domyślny separator par bajtów w przypadku wyświetlania obiektu w postaci ciągu znaków.
        /// </summary>
        [XmlIgnore]
        public String DefaultSeparator
        {
            get { return _defaultSeparator; }
            set { _defaultSeparator = value; }
        }

        /// <summary>
        /// Czy wyświetlane litery mają być wielkie.
        /// </summary>
        [XmlIgnore]
        public Boolean UpperCase
        {
            get { return _upperCase; }
            set { _upperCase = value; }
        }

        /// <summary>
        /// Przyjmuje i zwraca dane w postaci surowej - tablica bajtów.
        /// </summary>
        [XmlIgnore]
        public Byte[] ByteData
        {
            get { return _data; }
            set { _data = value; }
        }

        /// <summary>
        /// Przyjmuje i zwraca dane w postaci ciągu znaków.
        /// </summary>
        [XmlAttribute("as_string")]
        public String StringData
        {
            get { return ToString(_defaultSeparator); }
            set { FromString(value); }
        }

        /// <summary>
        /// Zwraca liczbę bajtów.
        /// </summary>
        [XmlIgnore]
        public int Length
        {
            get { return _data.Length; }
            set { }
        }

        /// <summary>
        /// Zwraca tablicę bajtów z bajtami w odwrotnej kolejności niż bieżąca.
        /// Nie modyfikuje bieżącej tablicy.
        /// </summary>
        [XmlIgnore]
        public ByteArray Reversed
        {
            get
            {
                ByteArray reversed = new ByteArray(Length);

                for (int i = 0; i < Length; i++)
                {
                    reversed[Length - i - 1] = this[i];
                }

                return reversed;
            }
            set { }
        }

        /// <summary>
        /// Domyślny konstruktor. Ustawia separator na spację i tryb wyświetlania litera na wielkie.
        /// </summary>
        public ByteArray()
        {
            _data = new byte[0];
            _defaultSeparator = " ";
            _upperCase = true;
        }

        /// <summary>
        /// Konstruktor z automatycznym stoworzeniem pustej tablicy bajtów o określonej długości.
        /// Ustawia separator na spację i tryb wyświetlania litera na wielkie.
        /// </summary>
        /// <param name="length">Długość tablicy bajtów.</param>
        public ByteArray(int length)
        {
            ByteData = new byte[length];
            _defaultSeparator = " ";
            _upperCase = true;
        }

        /// <summary>
        /// Konstruktor z automatycznym stoworzeniem tablicy bajtów o określonej długości.
        /// Tablica wypełniana jest bajtami zgodnie z wartością wskazaną w drugim parametrze.
        /// </summary>
        /// <param name="length">Długość tablicy bajtów.</param>
        /// <param name="fillByte">Wskazuje jaką wartość przypisać wszystkim bajtom w tablicy.</param>
        public ByteArray(int length, byte fillByte)
        {
            ByteData = new byte[length];

            for (int i = 0; i < Length; i++)
                this[i] = fillByte;

            _defaultSeparator = " ";
            _upperCase = true;
        }

        /// <summary>
        /// Konstruktor z automatycznym przypisaniem wartości poprzez podanie tablicy bajtów.
        /// Ustawia separator na spację i tryb wyświetlania litera na wielkie.
        /// </summary>
        /// <param name="byteData">Dane w postaci tablicy bajtów.</param>
        public ByteArray(Byte[] byteData)
        {
            if (byteData == null)
                ByteData = new byte[0];
            else
                ByteData = byteData;
            _defaultSeparator = " ";
            _upperCase = true;
        }

        /// <summary>
        /// Konstruktor z automatycznym przypisaniem wartości poprzez podanie ciągu znaków.
        /// Ustawia separator na spację i tryb wyświetlania litera na wielkie.
        /// </summary>
        /// <param name="stringData">Dane w postaci ciągu znaków.</param>
        public ByteArray(String stringData)
        {
            StringData = stringData;
            _defaultSeparator = " ";
            _upperCase = true;
        }

        /// <summary>
        /// Konstruktor z automatycznym przypisaniem wartości poprzez podanie tablicy bajtów
        /// oraz ustawieniem innych niż domyślne wartości separatora i wielkości liter.
        /// </summary>
        /// <param name="byteData">Dane w postaci tablicy bajtów.</param>
        /// <param name="separator">Separator oddzielający pary bajtów przy wyświetlaniu bajtów w postaci ciągu znaków.</param>
        /// <param name="upperCase">Czy litery mają być wielkie.</param>
        public ByteArray(Byte[] byteData, String separator, Boolean upperCase)
        {
            if (byteData == null)
                ByteData = new byte[0];
            else
                ByteData = byteData;
            _defaultSeparator = separator;
            _upperCase = upperCase;
        }

        /// <summary>
        /// Konstruktor z automatycznym przypisaniem wartości poprzez podanie ciągu znaków
        /// oraz ustawieniem innych niż domyślne wartości separatora i wielkości liter.
        /// </summary>
        /// <param name="stringData">Dane w postaci ciągu znaków.</param>
        /// <param name="separator">Separator oddzielający pary bajtów przy wyświetlaniu bajtów w postaci ciągu znaków.</param>
        /// <param name="upperCase">Czy litery mają być wielkie.</param>
        public ByteArray(String stringData, String separator, Boolean upperCase)
        {
            StringData = stringData;
            _defaultSeparator = separator;
            _upperCase = upperCase;
        }

        public static ByteArray GenerateRandom(int length)
        {
            ByteArray result = new ByteArray(length);
            result.Randomize(_random);
            return result;
        }

        public static ByteArray GenerateRandom(int length, ByteArray sourceBytes)
        {
            ByteArray result = new ByteArray(length);
            result.Randomize(_random, sourceBytes);
            return result;
        }

        /// <summary>
        /// Zwraca tablicę bajtów w postaci ciągu znaków - par bajtów oddzielonych domyślnym separatorem.
        /// </summary>
        /// <returns>Ciąg znaków - par bajtów oddzielonych domyślnym separatorem.</returns>
        override public String ToString()
        {
            return ToString(_defaultSeparator);
        }

        /// <summary>
        /// Zwraca tablicę bajtów w postaci ciągu znaków - par bajtów oddzielonych separatorem podanym jako parametr.
        /// </summary>
        /// <param name="separator">Separator par bajtów.</param>
        /// <returns>Ciąg znaków - par bajtów oddzielonych podanym separatorem.</returns>
        public String ToString(String separator)
        {
            String hexified = "";
            String format = "{0:x2}";
            if (_upperCase)
                format = "{0:X2}";

            for (int i = 0; i < this.Length; i++)
            {
                if (hexified.Length != 0)
                    hexified += separator;
                hexified += String.Format(format, this[i]);
            }
            return hexified;
        }

        /// <summary>
        /// Zwraca tablicę bajtów w postaci ciągu znaków - par bajtów oddzielonych separatorem podanym jako parametr.
        /// </summary>
        /// <param name="separator">Separator par bajtów.</param>
        /// <param name="upperCase">Czy litery mają być wielkie.</param>
        /// <returns>Ciąg znaków - par bajtów oddzielonych podanym separatorem o określonej wielkości liter a-f.</returns>
        public String ToString(String separator, Boolean upperCase)
        {
            String hexified = "";
            String format = "{0:x2}";
            if (upperCase)
                format = "{0:X2}";

            for (int i = 0; i < this.Length; i++)
            {
                if (hexified.Length != 0)
                    hexified += separator;
                hexified += String.Format(format, this[i]);
            }
            return hexified;
        }

        /// <summary>
        /// Ustawia bieżącą wartość obiektu, konwertując podany ciąg znaków na tablicę bajtów.
        /// Brane będą tylko znaki będące cyframi szesnastkowymi.
        /// </summary>
        /// <param name="stringValue">Ciąg znaków do konwersji na tablicę bajtów. Musi zawierać parzystą liczbę cyfr szesnastkowych.</param>
        private void FromString(String stringValue)
        {
            String onlyHexDigits = "";
            String hexDigits = "0123456789abcdefABCDEF";

            //najpierw wywalamy znaki nie będące poprawnymi cyframi w systemie szesnastkowym
            for (int i = 0; i < stringValue.Length; i++)
            {
                if (hexDigits.Contains(new String(new Char[] { stringValue[i] })))
                    onlyHexDigits += stringValue[i];
            }

            if ((onlyHexDigits.Length & 1) != 0)
                throw new ArgumentException("Ciąg znaków musi zawierać parzystą liczbę cyfr szesnastkowych.");

            String hex;
            int j = 0;
            _data = new Byte[onlyHexDigits.Length / 2];
            for (int i = 0; i < _data.Length; i++)
            {
                hex = new String(new Char[] { onlyHexDigits[j], onlyHexDigits[j + 1] });
                _data[i] = HexToByte(hex);
                j = j + 2;
            }
        }

        /// <summary>
        /// Konwertuje ciąg znaków na pojedynczy bajt.
        /// </summary>
        /// <param name="hex">Ciąg znaków składający się z jednej lub dwóch cyfr szesnastkowych.</param>
        /// <returns>Ciąg znaków skonwertowany na bajt.</returns>
        private byte HexToByte(string hex)
        {
            if (hex.Length > 2 || hex.Length <= 0)
                throw new ArgumentException("Ciąg wejściowy musi mieć 1 lub 2 znaki.");
            byte newByte = byte.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            return newByte;
        }

        /// <summary>
        /// Ustawia nową długość tablicy bajtów. Jeśli tablica jest powiększana nowe bajty mają warość 0x00.
        /// </summary>
        /// <param name="length">Nowa długość tablicy bajtów.</param>
        public void SetLength(int length)
        {
            byte[] oldData = _data;
            _data = new byte[length];
            for (int i = 0; i < _data.Length && i < oldData.Length; i++)
            {
                _data[i] = oldData[i];
            }
        }

        /// <summary>
        /// Ustawia nową długość tablicy bajtów. Jeśli tablica jest powiększana nowe bajty mają wartość
        /// taką jak wskazana przez drugi parametr.
        /// </summary>
        /// <param name="length">Nowa długość tablicy bajtów.</param>
        /// /// <param name="fillByte">Wskazuje jaką wartość przypisać nowym bajtom.</param>
        public void SetLength(int length, byte fillByte)
        {
            byte[] oldData = _data;
            _data = new byte[length];
            for (int i = 0; i < _data.Length; i++)
            {
                if (i < oldData.Length)
                    _data[i] = oldData[i];
                else
                    _data[i] = fillByte;
            }
        }

        /// <summary>
        /// Xoruje wszystkie bajty tablicy z podaną tablicą.
        /// </summary>
        /// <param name="xorWith">Z tą tablicą będzie wykonana operacja XOR tablicy bieżącej.</param>
        /// <returns>Wynikowa tablica.</returns>
        public ByteArray XOR(ByteArray xorWith)
        {
            if (this.Length != xorWith.Length)
                throw new Exception("Tablice bajtów mają różną długość");

            ByteArray xorred = new ByteArray(this.ByteData.Length);

            for (int i = 0; i < this.Length; i++)
            {
                xorred[i] = (byte)(this[i] ^ xorWith[i]);
            }

            return xorred;
        }

        /// <summary>
        /// Zwraca tablicę najmniej znaczących bajtów (z końca tablicy).
        /// </summary>
        /// <param name="length">Liczba bajtów.</param>
        /// <returns>Tablica bajtów.</returns>
        public ByteArray LSB(int length)
        {
            ByteArray lsb = new ByteArray(length);
            int i, j;
            if (length > this.Length)
                length = this.Length;

            for (j = 0, i = length; i > 0; j++, i--)
            {
                lsb[j] = this[this.Length - i];
            }

            return lsb;
        }

        /// <summary>
        /// Zwraca tablicę najbardziej znaczących bajtów (z początku tablicy).
        /// </summary>
        /// <param name="length">Liczba bajtów.</param>
        /// <returns>Tablica bajtów.</returns>
        public ByteArray MSB(int length)
        {
            ByteArray msb = new ByteArray(length);

            for (int i = 0; i < length; i++)
            {
                msb[i] = this[i];
            }

            return msb;
        }

        /// <summary>
        /// Odszyfrowanie danych podanych jako pierwszy parametr z bieżącą tablicą służącą jako klucz.
        /// Stosowany algorytm to DES.
        /// </summary>
        /// <param name="dataToDecode">Dane do odszyfrowania.</param>
        /// <param name="initVector">Wektor inicjalizujący.</param>
        /// <param name="paddingMode">Tryb dopełniania.</param>
        /// <param name="cipherMode">Tryb działania algorytmu.</param>
        /// <returns>Odszyfrowane dane.</returns>
        public ByteArray SimpleDecodeAsKey(ByteArray dataToDecode, ByteArray initVector, PaddingMode paddingMode, CipherMode cipherMode)
        {
            DESCryptoServiceProvider tdes = new DESCryptoServiceProvider();
            tdes.Padding = paddingMode;
            tdes.Mode = cipherMode;
            MemoryStream inputStream = new MemoryStream();
            CryptoStream decStream = new CryptoStream(inputStream, tdes.CreateDecryptor(this.ByteData, initVector.ByteData), CryptoStreamMode.Write);
            decStream.Write(dataToDecode.ByteData, 0, dataToDecode.Length);
            decStream.FlushFinalBlock();
            inputStream.Position = 0;
            ByteArray decData = new ByteArray(inputStream.ToArray());
            return decData;
        }

        /// <summary>
        /// Odszyfrowanie danych z bieżącej tablicy za pomocą klucza podanego jako pierwszy parametr.
        /// Stosowany algorytm to DES.
        /// </summary>
        /// <param name="key">Klucz.</param>
        /// <param name="initVector">Wektor inicjalizujący.</param>
        /// <param name="paddingMode">Tryb dopełniania.</param>
        /// <param name="cipherMode">Tryb działania algorytmu.</param>
        /// <returns>Odszyfrowane dane.</returns>
        public ByteArray SimpleDecodeAsData(ByteArray key, ByteArray initVector, PaddingMode paddingMode, CipherMode cipherMode)
        {
            DESCryptoServiceProvider tdes = new DESCryptoServiceProvider();
            tdes.Padding = paddingMode;
            tdes.Mode = cipherMode;
            MemoryStream inputStream = new MemoryStream();
            CryptoStream decStream = new CryptoStream(inputStream, tdes.CreateDecryptor(key.ByteData, initVector.ByteData), CryptoStreamMode.Write);
            decStream.Write(this.ByteData, 0, this.Length);
            decStream.FlushFinalBlock();
            inputStream.Position = 0;
            ByteArray decData = new ByteArray(inputStream.ToArray());
            return decData;

        }

        /// <summary>
        /// Odszyfrowanie danych podanych jako pierwszy parametr z bieżącą tablicą służącą jako klucz.
        /// Stosowany algorytm to 3DES.
        /// </summary>
        /// <param name="dataToDecode">Dane do odszyfrowania.</param>
        /// <param name="initVector">Wektor inicjalizujący.</param>
        /// <param name="paddingMode">Tryb dopełniania.</param>
        /// <param name="cipherMode">Tryb działania algorytmu.</param>
        /// <returns>Odszyfrowane dane.</returns>
        public ByteArray DecodeAsKey(ByteArray dataToDecode, ByteArray initVector, PaddingMode paddingMode, CipherMode cipherMode)
        {
            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Padding = paddingMode;
            tdes.Mode = cipherMode;
            MemoryStream inputStream = new MemoryStream();
            CryptoStream decStream = new CryptoStream(inputStream, tdes.CreateDecryptor(this.ByteData, initVector.ByteData), CryptoStreamMode.Write);
            decStream.Write(dataToDecode.ByteData, 0, dataToDecode.Length);
            decStream.FlushFinalBlock();
            inputStream.Position = 0;
            ByteArray decData = new ByteArray(inputStream.ToArray());
            return decData;
        }

        /// <summary>
        /// Odszyfrowanie danych z bieżącej tablicy za pomocą klucza podanego jako pierwszy parametr.
        /// Stosowany algorytm to 3DES.
        /// </summary>
        /// <param name="key">Klucz.</param>
        /// <param name="initVector">Wektor inicjalizujący.</param>
        /// <param name="paddingMode">Tryb dopełniania.</param>
        /// <param name="cipherMode">Tryb działania algorytmu.</param>
        /// <returns>Odszyfrowane dane.</returns>
        public ByteArray DecodeAsData(ByteArray key, ByteArray initVector, PaddingMode paddingMode, CipherMode cipherMode)
        {
            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Padding = paddingMode;
            tdes.Mode = cipherMode;
            MemoryStream inputStream = new MemoryStream();
            CryptoStream decStream = new CryptoStream(inputStream, tdes.CreateDecryptor(key.ByteData, initVector.ByteData), CryptoStreamMode.Write);
            decStream.Write(this.ByteData, 0, this.Length);
            decStream.FlushFinalBlock();
            inputStream.Position = 0;
            ByteArray decData = new ByteArray(inputStream.ToArray());
            return decData;

        }

        /// <summary>
        /// Szyfrowanie danych podanych jako pierwszy parametr z bieżącą tablicą służącą jako klucz.
        /// Stosowany algorytm to DES.
        /// </summary>
        /// <param name="dataToEncode">Dane do zaszyfrowania.</param>
        /// <param name="initVector">Wektor inicjalizujący.</param>
        /// <param name="paddingMode">Tryb dopełniania.</param>
        /// <param name="cipherMode">Tryb działania algorytmu.</param>
        /// <returns>Zaszyfrowane dane.</returns>
        public ByteArray SimpleEncodeAsKey(ByteArray dataToEncode, ByteArray initVector, PaddingMode paddingMode, CipherMode cipherMode)
        {
            DESCryptoServiceProvider tdes = new DESCryptoServiceProvider();
            tdes.Padding = paddingMode;
            tdes.Mode = cipherMode;
            MemoryStream inputStream = new MemoryStream();
            CryptoStream encStream = new CryptoStream(inputStream, tdes.CreateEncryptor(this.ByteData, initVector.ByteData), CryptoStreamMode.Write);
            encStream.Write(dataToEncode.ByteData, 0, dataToEncode.Length);
            encStream.FlushFinalBlock();
            byte[] encData = new byte[inputStream.Position];
            inputStream.Position = 0;
            inputStream.Read(encData, 0, encData.Length);
            encStream.Close();
            return new ByteArray(encData);
        }

        /// <summary>
        /// Zaszyfrowanie danych z bieżącej tablicy za pomocą klucza podanego jako pierwszy parametr.
        /// Stosowany algorytm to DES.
        /// </summary>
        /// <param name="key">Klucz.</param>
        /// <param name="initVector">Wektor inicjalizujący.</param>
        /// <param name="paddingMode">Tryb dopełniania.</param>
        /// <param name="cipherMode">Tryb działania algorytmu.</param>
        /// <returns>Zaszyfrowane dane.</returns>
        public ByteArray SimpleEncodeAsData(ByteArray key, ByteArray initVector, PaddingMode paddingMode, CipherMode cipherMode)
        {
            DESCryptoServiceProvider tdes = new DESCryptoServiceProvider();
            tdes.Padding = paddingMode;
            tdes.Mode = cipherMode;
            MemoryStream inputStream = new MemoryStream();
            CryptoStream encStream = new CryptoStream(inputStream, tdes.CreateEncryptor(key.ByteData, initVector.ByteData), CryptoStreamMode.Write);
            encStream.Write(this.ByteData, 0, this.Length);
            encStream.FlushFinalBlock();
            byte[] encData = new byte[inputStream.Position];
            inputStream.Position = 0;
            inputStream.Read(encData, 0, encData.Length);
            encStream.Close();
            return new ByteArray(encData);
        }

        /// <summary>
        /// Szyfrowanie danych podanych jako pierwszy parametr z bieżącą tablicą służącą jako klucz.
        /// Stosowany algorytm to 3DES.
        /// </summary>
        /// <param name="dataToEncode">Dane do zaszyfrowania.</param>
        /// <param name="initVector">Wektor inicjalizujący.</param>
        /// <param name="paddingMode">Tryb dopełniania.</param>
        /// <param name="cipherMode">Tryb działania algorytmu.</param>
        /// <returns>Zaszyfrowane dane.</returns>
        public ByteArray EncodeAsKey(ByteArray dataToEncode, ByteArray initVector, PaddingMode paddingMode, CipherMode cipherMode)
        {
            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Padding = paddingMode;
            tdes.Mode = cipherMode;
            MemoryStream inputStream = new MemoryStream();
            CryptoStream encStream = new CryptoStream(inputStream, tdes.CreateEncryptor(this.ByteData, initVector.ByteData), CryptoStreamMode.Write);
            encStream.Write(dataToEncode.ByteData, 0, dataToEncode.Length);
            encStream.FlushFinalBlock();
            byte[] encData = new byte[inputStream.Position];
            inputStream.Position = 0;
            inputStream.Read(encData, 0, encData.Length);
            encStream.Close();
            return new ByteArray(encData);
        }

        /// <summary>
        /// Szyfrowanie danych z bieżącej tablicy za pomocą klucza podanego jako pierwszy parametr.
        /// Stosowany algorytm to 3DES.
        /// </summary>
        /// <param name="key">Klucz.</param>
        /// <param name="initVector">Wektor inicjalizujący.</param>
        /// <param name="paddingMode">Tryb dopełniania.</param>
        /// <param name="cipherMode">Tryb działania algorytmu.</param>
        /// <returns>Zaszyfrowane dane.</returns>
        public ByteArray EncodeAsData(ByteArray key, ByteArray initVector, PaddingMode paddingMode, CipherMode cipherMode)
        {
            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Padding = paddingMode;
            tdes.Mode = cipherMode;
            MemoryStream inputStream = new MemoryStream();
            CryptoStream encStream = new CryptoStream(inputStream, tdes.CreateEncryptor(key.ByteData, initVector.ByteData), CryptoStreamMode.Write);
            encStream.Write(this.ByteData, 0, this.Length);
            encStream.FlushFinalBlock();
            byte[] encData = new byte[inputStream.Position];
            inputStream.Position = 0;
            inputStream.Read(encData, 0, encData.Length);
            encStream.Close();
            return new ByteArray(encData);
        }

        /// <summary>
        /// Zwraca skrót bieżących danych uzyskany algorytmem SHA512.
        /// </summary>
        /// <returns>Tablica ze skrótem.</returns>
        public ByteArray Hash()
        {
            return new ByteArray(new SHA512Managed().ComputeHash(ByteData));
        }

        /// <summary>
        /// Operator dodawania dwóch obiektów ByteArray. Bajty z drugiego doklejane są na koniec pierwszego.
        /// </summary>
        /// <param name="b1">Lewy składnik operacji.</param>
        /// <param name="b2">Prawy składnik operacji</param>
        /// <returns>Wynikowa tablica bajtów.</returns>
        public static ByteArray operator +(ByteArray b1, ByteArray b2)
        {
            int b1Length;
            int b2Length;

            if (b1 == null)
                b1Length = 0;
            else
                b1Length = b1.Length;

            if (b2 == null)
                b2Length = 0;
            else
                b2Length = b2.Length;

            byte[] newData = new byte[b1Length + b2Length];
            int i;

            for (i = 0; i < b1Length; i++)
            {
                newData[i] = b1[i];
            }

            for (i = 0; i < b2Length; i++)
            {
                newData[b1.Length + i] = b2[i];
            }

            ByteArray newValue = new ByteArray(newData);
            return newValue;
        }

        /// <summary>
        /// Operator dodawania obiektu string do ByteArray. Bajty z drugiego doklejane są na koniec pierwszego.
        /// </summary>
        /// <param name="b1">Lewy składnik operacji.</param>
        /// <param name="b2">Prawy składnik operacji</param>
        /// <returns>Wynikowa tablica bajtów.</returns>
        public static ByteArray operator +(ByteArray b1, string b2)
        {
            return b1 + new ByteArray(b2);
        }

        /// <summary>
        /// Operator dodawania bajtu do tablicy ByteArray. Bajt doklejany jest na końcu tablicy.
        /// </summary>
        /// <param name="b1">Tablica bajtów.</param>
        /// <param name="addByte">Bajt do dodania.</param>
        /// <returns>Wynikowa tablica bajtów.</returns>
        public static ByteArray operator +(ByteArray b1, Byte addByte)
        {
            int b1Length;

            if (b1 == null)
                b1Length = 0;
            else
                b1Length = b1.Length;

            byte[] newData = new byte[b1Length + 1];
            int i;

            for (i = 0; i < b1Length; i++)
            {
                newData[i] = b1[i];
            }

            newData[newData.Length - 1] = addByte;

            ByteArray newValue = new ByteArray(newData);
            return newValue;
        }

        /// <summary>
        /// Operator dodawania bajtu do tablicy ByteArray. Bajt doklejany jest na początku tablicy.
        /// </summary>
        /// <param name="b1">Tablica bajtów.</param>
        /// <param name="addByte">Bajt do dodania na początku tablicy.</param>
        /// <returns>Wynikowa tablica bajtów.</returns>
        public static ByteArray operator +(Byte addByte, ByteArray b1)
        {
            int b1Length;

            if (b1 == null)
                b1Length = 0;
            else
                b1Length = b1.Length;

            byte[] newData = new byte[b1Length + 1];
            int i;

            for (i = 0; i < b1Length; i++)
            {
                newData[i + 1] = b1[i];
            }

            newData[0] = addByte;

            ByteArray newValue = new ByteArray(newData);
            return newValue;
        }

        /// <summary>
        /// Zwraca wycinek tablicy bajtów o podanym początku i długości.
        /// </summary>
        /// <param name="offset">Początek wycinka.</param>
        /// <param name="length">Długośc wycinka.</param>
        /// <returns>Tablica zawierająca wycięte bajty.</returns>
        public ByteArray Extract(int offset, int length)
        {
            ByteArray result = new ByteArray();
            int i, j;

            if (offset > this.Length || offset < 0 || length <= 0)
                return result;

            for (i = offset, j = 0; j < length && i < this.Length; i++, j++)
                result[j] = this[i];

            return result;
        }

        /// <summary>
        /// Zwraca wycinek tablicy bajtów o podanym początku, do końca tablicy.
        /// </summary>
        /// <param name="offset">Początek wycinka.</param>
        /// <returns>Tablica zawierająca wycięte bajty.</returns>
        public ByteArray Extract(int offset)
        {
            ByteArray result = new ByteArray();
            int i, j;

            if (offset > this.Length || offset < 0 || this.Length == 0)
                return result;

            for (i = offset, j = 0; i < this.Length; i++, j++)
                result[j] = this[i];

            return result;
        }

        /// <summary>
        /// Zwraca tablicę obiektów ByteArray będących efektem podzielenie bieżącej tablicy na równe części.
        /// Jeśli ostatnia część jest mniejsza od wskazanej długości, można wskazać czy ma ona być dopełniana
        /// do tej wielkości, a jesli tak to jaką wartość mają przyjąć dodane bajty.
        /// </summary>
        /// <param name="splitLength">Określa jak długie mają być wynikowe części.</param>
        /// <param name="padding">Czy dopełniać ostatnią część do długości podziału.</param>
        /// <param name="paddingByte">Wskazuje jaką wartość mają mieć bajty dodane w ostatniej części.</param>
        /// <returns>Wynik podziału bieżącej tablicy na części o podanej długości.</returns>
        public ByteArray[] Slice(int splitLength, bool padding, byte paddingByte)
        {
            int slices = Length / splitLength;
            if (Length % splitLength != 0)
                slices++;

            ByteArray[] result = new ByteArray[slices];
            for (int i = 0; i < slices; i++)
            {
                ByteArray slice = Extract(i * splitLength, splitLength);
                if (slice.Length < splitLength && padding)
                {
                    slice.SetLength(splitLength, paddingByte);
                }
                result[i] = slice;
            }
            return result;
        }

        /// <summary>
        /// Zwraca pozycję podanego bajtu w tablicy, licząc od początku.
        /// </summary>
        /// <param name="searchByte">Bajt, którego pozycja ma być znaleziona.</param>
        /// <returns>Pozycja bajtu w tablicy lub -1 jeśli bajtu nie znaleziono.</returns>
        public Int32 IndexOf(byte searchByte)
        {
            for (int i = 0; i < Length; i++)
            {
                if (this[i] == searchByte)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Wywala z tablicy bajty o określonych wartościach z poczatku i/lub końca.
        /// Usuwanie bajtów zatrzymuje się po napotkaniu pierwszego bajtu spoza bajtów
        /// określonych jako parametr.
        /// </summary>
        /// <param name="bytesToTrim">Bajty, które mają być usunięte z tablicy.</param>
        /// <param name="fromStart">Czy bajty mają być usuwane z początku tablicy.</param>
        /// <param name="fromEnd">Czy bajty mają być usuwane z końca tablicy.</param>
        public ByteArray Trim(ByteArray bytesToTrim, Boolean fromStart, Boolean fromEnd)
        {
            Int32 trimStartLength = 0;
            Int32 trimEndLength = 0;

            if (fromStart)
            {
                for (int i = 0; i < Length && bytesToTrim.IndexOf(this[i]) >= 0; i++, trimStartLength++) ;
            }

            if (fromEnd)
            {
                for (int i = Length - 1; i >= 0 && bytesToTrim.IndexOf(this[i]) >= 0; i--, trimEndLength++) ;
            }

            return Extract(trimStartLength, Length - trimEndLength - trimStartLength);
        }

        /// <summary>
        /// Wywala z tablicy bajty 0x00 (z poczatku i końca).
        /// </summary>
        public ByteArray TrimZeros()
        {
            return Trim(new ByteArray("00"), true, true);
        }

        /// <summary>
        /// Zwraca tablicę obiektów ByteArray będących efektem podzielenie bieżącej tablicy na równe części.
        /// Jeśli ostatnia część jest mniejsza od wskazanej długości, dopełniana jest ona do tej wielkości
        /// bajtami o wartości 0x00.
        /// </summary>
        /// <param name="splitLength">Określa jak długie mają być wynikowe części.</param>
        /// <returns>Wynik podziału bieżącej tablicy na części o podanej długości.</returns>
        public ByteArray[] Slice(int splitLength)
        {
            return Slice(splitLength, true, 0x00);
        }

        /// <summary>
        /// Kopiuje bajty ze wskazanej tablicy do tablicy bieżącej. Trzeba wskazać pozycję,
        /// od której zacząć pobieranie bajtów, pozycję, od której zacząć wstawianie bajtów oraz
        /// liczbę bajtów do skopiowania.
        /// </summary>
        /// <param name="srcData">Źródłowa tablica bajtów.</param>
        /// <param name="srcOffset">Pozycja, od której zacząć pobieranie bajtów.</param>
        /// <param name="dstOffset">Pozycja, od której zacząć wstawiania bajtów.</param>
        /// <param name="length">Liczba bajtów do skopiowania.</param>
        public void CopyFrom(ByteArray srcData, int srcOffset, int dstOffset, int length)
        {
            int src, dst, i;
            for (src = srcOffset, dst = dstOffset, i = 0; i < length; src++, dst++, i++)
            {
                this[dst] = srcData[src];
            }
        }

        /// <summary>
        /// Wypełnia bieżącą tablicę losowymi wartościami.
        /// </summary>
        /// <param name="randomGenerator">Generator liczb losowych.</param>
        public void Randomize(Random randomGenerator)
        {
            randomGenerator.NextBytes(_data);
        }

        /// <summary>
        /// Wypełnia bieżącą tablicę losowymi wartościami pobranymi ze źródłowej tablicy bajtów.
        /// </summary>
        /// <param name="randomGenerator">Generator liczb losowych.</param>
        /// <param name="sourceBytes">Tablica bajtów, z której czerpane są wartości bajtów.</param>
        public void Randomize(Random randomGenerator, ByteArray sourceBytes)
        {
            int i;
            for (i = 0; i < this.Length; i++)
            {
                this[i] = sourceBytes[randomGenerator.Next(0, sourceBytes.Length - 1)];
            }
        }

        /// <summary>
        /// Zapisuje zawartość tablicy do pliku.
        /// </summary>
        /// <param name="filepath">Ścieżka do pliku.</param>
        public void SaveToFile(String filepath)
        {
            using (FileStream file = new FileStream(filepath, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter bw = new BinaryWriter(file))
                {
                    bw.Write(_data);
                    bw.Close();
                }
                file.Close();
            }
        }

        /// <summary>
        /// Ładuje zawartość pliku do tablicy bajtów.
        /// </summary>
        /// <param name="filepath">Ścieżka do pliku.</param>
        /// <returns>Zawartośćpliku w postaci tablicy bajtów.</returns>
        static public ByteArray LoadFromFile(String filepath)
        {
            return new ByteArray(File.ReadAllBytes(filepath));
        }

        /// <summary>
        /// Redefinicja wymagana przy redefiniowaniu metody Equals.
        /// Nie jest i nie powinna być używana.
        /// </summary>
        /// <returns>Zawsze wartość 0.</returns>
        public override int GetHashCode()
        {
            return 0;
        }

        /// <summary>
        /// Porównanie dwóch tablicy bieżącej z podaną jako parametr.
        /// </summary>
        /// <param name="obj">Tablica bajtów do porównania.</param>
        /// <returns>Wynik porównania (wartość logiczna).</returns>
        public override bool Equals(object obj)
        {
            ByteArray compareTo = (ByteArray)obj;

            if (this.Length != compareTo.Length)
                return false;

            for (int i = 0; i < this.Length; i++)
            {
                if (this[i] != compareTo[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Inkrementuje wartość ByteArraya
        /// </summary>
        /// <returns>true - jeśli przekroczono zakres i rozpoczęto od zera</returns>
        public bool Increment()
        {
            bool overflow = false;
            for (int i = Length - 1; i >= 0; i--)
            {
                if (this[i] == 0xFF)
                {
                    this[i] = 0x00;
                    overflow = true;
                    continue;
                }

                this[i]++;
                overflow = false;
                break;
            }

            return overflow;
        }

        /// <summary>
        /// Zamienia tablicę bajtów na tekst
        /// </summary>
        /// <returns></returns>
        public string ToText()
        {
            var byteString = Encoding.UTF8.GetString(ByteData);
            string result = byteString.Replace('\0', '.');
            return result;
        }
    }

}