using System.Collections;
using System.Collections.Generic;
using Subsembly.SmartCard;


namespace Utilities
{
    public interface IMainApplication
    {
        /// <summary>
        /// Przekazanie do głównej aplikacji żądania wykonania polecenia
        /// </summary>
        /// <param name="command">Żądane polecenie</param>
        /// <param name="parameters">Parametry wywoływanego polecenia</param>
        /// <returns>Wynik wykonania polecenia</returns>
        Hashtable Exec(string command, Hashtable parameters);

        /// <summary>
        /// Lista załadowanych wtyczek
        /// </summary>
        List<IPlugin> Plugins { get; }

        /// <summary>
        /// Terminal kart elektronicznych
        /// </summary>
        IEncoder Encoder { get; }

        /// <summary>
        /// Zarządca terminali kart elektronicznych
        /// </summary>
        CardTerminalManager TerminalManager { get; }

        /// <summary>
        /// Dane aktualnie włożonej karty
        /// </summary>
        CardType CardType { get; }
    }
}
