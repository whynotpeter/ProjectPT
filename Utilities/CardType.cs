using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows.Forms;
using System.Xml;

namespace Utilities
{
    public enum CardTypes
    {
        Unknown = 0,
        JavaCard = 1,
        GSM = 2,
        IAS = 4,
        Cosmo = 8,
        MIFARE = 16
        // pomyśleć jakie typy kart obsługiwać i na ile je rozróżniać
    } ;

    public class CardType
    {
        public string Name;
        public ByteArray ATR;
        public ByteArray ATRmask;
        public ByteArray CMaid;
        public ByteArray ISOaid;
        public List<ByteArray> OtherAIDs;
        public string PkcsDll;

        public static List<CardType> Types = new List<CardType>();

        public CardType(string name, string atr, string mask, string cm, string iso, string otherAIDs, string pkcsDll = "")
        {
            Name = name;
            ATR = new ByteArray(atr);
            ATRmask = new ByteArray(mask);
            CMaid = new ByteArray(cm);
            ISOaid = new ByteArray(iso);
            OtherAIDs = new List<ByteArray>();
            try
            {
                OtherAIDs.Add(CMaid);
                string[] aids = otherAIDs.Split(';');
                foreach (var s in aids)
                {
                    OtherAIDs.Add(new ByteArray(s));
                }
            }
            catch (Exception e)
            {
            }
            PkcsDll = pkcsDll;
        }

        public static CardType GetCardType(ByteArray atr)
        {
            foreach (CardType type in Types)
            {
                try
                {
                    bool next = false;
                    if (type.ATRmask.Length == 0)
                    {
                        continue;
                    }
                    for (int i = 0; i < type.ATRmask.ToString("", true).Length; i++)
                    {
                        if (type.ATRmask.ToString("", true)[i] == 'F')
                            if (atr.ToString("", true)[i] != type.ATR.ToString("", true)[i])
                            {
                                next = true;
                                break;
                            }
                    }
                    if (next) continue;
                    return type;
                }
                catch
                {
                    continue;
                }
            }
            return Types[0];
        }

        public static void FromXML(string fileName)
        {
            Types.Clear();
            Types.Add(new CardType("Nieznana", "", "", "", "", ""));
            XmlDocument xmlATRs;
            xmlATRs = new XmlDocument();
            xmlATRs.Load(fileName);
            XmlNodeList cardTypesList = xmlATRs.GetElementsByTagName("SmartCard")[0].ChildNodes;
            foreach (XmlNode cardType in cardTypesList)
            {
                string name = cardType.Name;
                string atr = cardType.Attributes["ATR"].Value;
                string mask = cardType.Attributes["ATRMask"].Value;
                string aid = cardType.Attributes["Aid"].Value;
                Types.Add(new CardType(name, atr, mask, aid, "", ""));
            }
        }

        public static void FromSQLite(string fileName)
        {
            Types.Clear();
            Types.Add(new CardType("Nieznana", "", "", "", "", ""));

            using (SQLiteConnection connection = new SQLiteConnection("URI=file:" + fileName))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT name, value, mask, aid, more_aid, iso, pkcs_dll FROM atrs";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            string name;
                            string atr;
                            string mask;
                            string aid;
                            string more_aid;
                            string iso;
                            string pkcs_dll;
                            while (reader.Read())
                            {
                                name = reader.GetString(0);
                                atr = reader.GetString(1);
                                mask = reader.GetString(2);
                                try
                                {
                                    aid = reader.GetString(3);
                                }
                                catch (InvalidCastException e)
                                {
                                    aid = "";
                                }
                                try
                                {
                                    more_aid = reader.GetString(4);
                                }
                                catch (InvalidCastException e)
                                {
                                    more_aid = "";
                                }
                                try
                                {
                                    iso = reader.GetString(5);
                                }
                                catch (InvalidCastException e)
                                {
                                    iso = "";
                                }
                                try
                                {
                                    pkcs_dll = reader.GetString(6);
                                }
                                catch (InvalidCastException e)
                                {
                                    pkcs_dll = "";
                                }
                                Types.Add(new CardType(name, atr, mask, aid, iso, more_aid, pkcs_dll));
                            }
                        }
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public static void SaveSQLite(string fileName)
        {
            using (SQLiteConnection connection = new SQLiteConnection("URI=file:" + fileName))
            {
                try
                {
                    connection.Open();
                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        string query = "INSERT INTO atrs (name, value, mask, aid) VALUES ('{0}', '{1}', '{2}', '{3}')";
                        foreach (CardType cardType in Types)
                        {
                            using (SQLiteCommand command = connection.CreateCommand())
                            {
                                command.CommandText = String.Format(query, cardType.Name, cardType.ATR, cardType.ATRmask, cardType.CMaid);
                                command.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                    }
                }
                catch (SQLiteException exception)
                {
                    MessageBox.Show("Nie udało się zapisać typów kart do bazy. " + exception.Message, "Błąd zapisu w bazie", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    connection.Close();
                }
            }
        }
    }
}
