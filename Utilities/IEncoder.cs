namespace Utilities
{
    public interface IEncoder
    {
        /// <summary>
        /// Wysyła do karty polecenie APDU
        /// </summary>
        /// <param name="command">Polecenie APDU</param>
        /// <param name="checkSuccessful">Ma sprawdzać, czy odpowiedź jest poprawna (90 00)</param>
        /// <returns>Odpowiedź na polecenie APDU</returns>
        ByteArray SendCommand(ByteArray command, bool checkSuccessful = true);

        /// <summary>
        /// Wybiera terminal
        /// </summary>
        /// <param name="namePattern">Nazwa terminala lub wzorzec nazwy</param>
        void Initialize(string namePattern);

        /// <summary>
        /// Łączy z kartą
        /// </summary>
        /// <param name="force">Wymuszenie zresetowania karty</param>
        /// <returns>Nr ATR karty</returns>
        ByteArray ConnectCard(bool force = false);

        /// <summary>
        /// Sprawdza gotowość terminala
        /// </summary>
        bool Ready { get; set; }

        /// <summary>
        /// Nazwa terminala
        /// </summary>
        string TerminalName { get; set; }
    }
}
