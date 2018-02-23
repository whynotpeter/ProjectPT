using System.Collections;
using WeifenLuo.WinFormsUI.Docking;

namespace Utilities
{
    public interface IPlugin
    {
        string PluginName { get; }
        string Description { get; }
        string Author { get; }
        string Version { get; }
        string Date { get; }

        DockContent PluginForm { get; }

        IMainApplication application { get; }

        /// <summary>
        /// Wywołuje wykonanie dowolnego polecenia z pluginu
        /// </summary>
        /// <param name="command">Nazwa polecenia</param>
        /// <param name="parameters">Parametry wejściowe</param>
        /// <returns>Wynik wywołania</returns>
        Hashtable Exec(string command, Hashtable parameters);

        /// <summary>
        /// Metoda wywoływana podczas uruchamiania pluginu
        /// </summary>
        /// <param name="initializationParameters">Parametry inicjalizacyjne</param>
        void PluginLoad(Hashtable initializationParameters);

        /// <summary>
        /// Metoda wywoływana podczas wyłączania pluginu
        /// </summary>
        /// <param name="parameters">Parametry końcowe</param>
        void PluginUnLoad(Hashtable parameters);

        /// <summary>
        /// Tworzy formę (lub upewnia się, że forma istnieje)
        /// </summary>
        /// <param name="force">Czy wymusić utworzenie nowej formy</param>
        void CreatePluginForm(bool force = false);
    }
}
