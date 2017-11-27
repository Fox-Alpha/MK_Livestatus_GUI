using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Xml;
using System.Security;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ClassHelperUtils
{                 
    /// <summary>
    /// Funktionen um einen Unixtimestamp in einen DateTime und umgekehrt zu konvertieren
    /// </summary>          
    public static class UnixDateTime
    {
        public static DateTimeOffset FromUnixTimeSeconds (long seconds)
        {
            if (seconds < -62135596800L || seconds > 253402300799L)
                throw new ArgumentOutOfRangeException ("seconds", seconds, "");

            return new DateTimeOffset (seconds * 10000000L + 621355968000000000L, TimeSpan.Zero);
        }

        public static DateTimeOffset FromUnixTimeMilliseconds (long milliseconds)
        {
            if (milliseconds < -62135596800000L || milliseconds > 253402300799999L)
                throw new ArgumentOutOfRangeException ("milliseconds", milliseconds, "");

            return new DateTimeOffset (milliseconds * 10000L + 621355968000000000L, TimeSpan.Zero);
        }

        public static long ToUnixTimeSeconds (this DateTimeOffset utcDateTime)
        {
            return utcDateTime.Ticks / 10000000L - 62135596800L;
        }

        public static long ToUnixTimeMilliseconds (this DateTimeOffset utcDateTime)
        {
            return utcDateTime.Ticks / 10000L - 62135596800000L;
        }

        //[Test]
        //public void UnixSeconds ()
        //{
        //    DateTime utcNow = DateTime.UtcNow;
        //    DateTimeOffset utcNowOffset = new DateTimeOffset (utcNow);

        //    long unixTimestampInSeconds = utcNowOffset.ToUnixTimeSeconds ();

        //    DateTimeOffset utcNowOffsetTest = UnixDateTime.FromUnixTimeSeconds (unixTimestampInSeconds);

        //    Assert.AreEqual (utcNowOffset.Year, utcNowOffsetTest.Year);
        //    Assert.AreEqual (utcNowOffset.Month, utcNowOffsetTest.Month);
        //    Assert.AreEqual (utcNowOffset.Date, utcNowOffsetTest.Date);
        //    Assert.AreEqual (utcNowOffset.Hour, utcNowOffsetTest.Hour);
        //    Assert.AreEqual (utcNowOffset.Minute, utcNowOffsetTest.Minute);
        //    Assert.AreEqual (utcNowOffset.Second, utcNowOffsetTest.Second);
        //}

        //[Test]
        //public void UnixMilliseconds ()
        //{
        //    DateTime utcNow = DateTime.UtcNow;
        //    DateTimeOffset utcNowOffset = new DateTimeOffset (utcNow);

        //    long unixTimestampInMilliseconds = utcNowOffset.ToUnixTimeMilliseconds ();

        //    DateTimeOffset utcNowOffsetTest = UnixDateTime.FromUnixTimeMilliseconds (unixTimestampInMilliseconds);

        //    Assert.AreEqual (utcNowOffset.Year, utcNowOffsetTest.Year);
        //    Assert.AreEqual (utcNowOffset.Month, utcNowOffsetTest.Month);
        //    Assert.AreEqual (utcNowOffset.Date, utcNowOffsetTest.Date);
        //    Assert.AreEqual (utcNowOffset.Hour, utcNowOffsetTest.Hour);
        //    Assert.AreEqual (utcNowOffset.Minute, utcNowOffsetTest.Minute);
        //    Assert.AreEqual (utcNowOffset.Second, utcNowOffsetTest.Second);
        //    Assert.AreEqual (utcNowOffset.Millisecond, utcNowOffsetTest.Millisecond);
        //}
    }

    /// <summary>
    /// Weitere hilfreiche Funktionen für Datei und Verzeichnisoperationen
    /// </summary>
    public static class PathUtil
    {
        /// <summary>
        /// Determines the relative path of the specified path relative to a base path.
        /// </summary>
        /// <param name="path">The path to make relative.</param>
        /// <param name="relBase">The base path.</param>
        /// <param name="throwOnDifferentRoot">If true, an exception is thrown for different roots, otherwise the source path is returned unchanged.</param>
        /// <returns>The relative path.</returns>
        public static string GetRelativePath (string path, string relBase, bool throwOnDifferentRoot = true)
        {
            // Use case-insensitive comparing of path names.
            // NOTE: This may be different on other systems.
            StringComparison sc = StringComparison.InvariantCultureIgnoreCase;

            // Are both paths rooted?
            if (!Path.IsPathRooted (path))
                throw new ArgumentException ("path argument is not a rooted path.");
            if (!Path.IsPathRooted (relBase))
                throw new ArgumentException ("relBase argument is not a rooted path.");

            // Do both paths share the same root?
            string pathRoot = Path.GetPathRoot (path);
            string baseRoot = Path.GetPathRoot (relBase);
            if (!string.Equals (pathRoot, baseRoot, sc))
            {
                if (throwOnDifferentRoot)
                {
                    throw new InvalidOperationException ("Both paths do not share the same root.");
                }
                else
                {
                    return path;
                }
            }

            // Cut off the path roots
            path = path.Substring (pathRoot.Length);
            relBase = relBase.Substring (baseRoot.Length);

            // Cut off the common path parts
            string [] pathParts = path.Split (Path.DirectorySeparatorChar);
            string [] baseParts = relBase.Split (Path.DirectorySeparatorChar);
            int commonCount;
            for (
                commonCount = 0;
                commonCount < pathParts.Length &&
                commonCount < baseParts.Length &&
                string.Equals (pathParts [commonCount], baseParts [commonCount], sc);
                commonCount++)
            {
            }

            // Add .. for the way up from relBase
            string newPath = "";
            for (int i = commonCount; i < baseParts.Length; i++)
            {
                newPath += ".." + Path.DirectorySeparatorChar;
            }

            // Append the remaining part of the path
            for (int i = commonCount; i < pathParts.Length; i++)
            {
                newPath = Path.Combine (newPath, pathParts [i]);
            }

            return newPath;
        }

        /// <summary>
        /// Ermittelt alle Verzeichnisse deren Alter mehr wie 24h beträgt
        /// Es werden nur die Unterverzeichnisse aus dem angegebenen Verzeichnis untersucht
        /// </summary>
        /// <param name="sourceDir">Verzeichnis das untersucht werden soll</param>
        /// <returns></returns>
        public static string [] GetDirGreaterThen24h (string sourceDir)
        {
            string [] Jobs = System.IO.Directory.GetDirectories (sourceDir, "", System.IO.SearchOption.TopDirectoryOnly);
            List<string> result = new List<string> ();

            foreach (string JobPath in Jobs)
            {
                string [] Jobbuf = System.IO.Directory.GetDirectories (JobPath + @"\buf\", "", System.IO.SearchOption.TopDirectoryOnly);
                foreach (string buf in Jobbuf)
                {
                    System.IO.DirectoryInfo diJob = new System.IO.DirectoryInfo (buf);
                    if (diJob.CreationTime < DateTime.Now.AddHours (-24))
                    {
                        Debug.WriteLine (@"Der Batch: " + buf + "ist älter wie 24h: " + diJob.CreationTime.ToString ());
                        result.Add (buf);
                    }
                }
            }
            return result.ToArray ();
        }
    }

    /// <summary>
    /// Ini configuration file provider
    /// </summary>
    public class IniFile
    {
        #region Fields (6)

        /// <summary>
        /// Inhalt der Datei
        /// </summary>
        private List<String> lines = new List<string> ();

        /// <summary>
        /// Voller Pfad und Name der Datei
        /// </summary>
        private String fileName = "";

        /// <summary>
        /// Gibt an, welche Zeichen als Kommentarbeginn
        /// gewertet werden sollen. Dabei wird das erste 
        /// Zeichen defaultmäßig für neue Kommentare
        /// verwendet.
        /// </summary>
        private String CommentCharacters = "#;";

        /// <summary>
        /// Regulärer Ausdruck für einen Kommentar in einer Zeile
        /// </summary>
        private String regCommentStr = "";

        /// <summary>
        /// Regulärer Ausdruck für einen Eintrag
        /// </summary>
        private Regex regEntry = null;

        /// <summary>
        /// Regulärer Ausdruck für einen Bereichskopf
        /// </summary>
        private Regex regCaption = null;

        #endregion

        #region Constructors (2)

        /// <summary>
        /// Leerer Standard-Konstruktor
        /// </summary>
        public IniFile ()
        {
            regCommentStr = @"(\s*[" + CommentCharacters + "](?<comment>.*))?";
            regEntry = new Regex (@"^[ \t]*(?<entry>([^=])+)=(?<value>([^=" + CommentCharacters + "])+)" + regCommentStr + "$");
            regCaption = new Regex (@"^[ \t]*(\[(?<caption>([^\]])+)\]){1}" + regCommentStr + "$");
        }

        /// <summary>
        /// Konstruktor, welcher sofort eine Datei einliest
        /// </summary>
        /// <param name="filename">Name der einzulesenden Datei</param>
        public IniFile (string filename) : this ()
        {
            //if (!File.Exists(filename)) throw new IOException("File " + filename + "  not found");
            fileName = filename;
        }

        #endregion

        #region Properties (2)

        /// <summary>
        /// Voller Name der Datei
        /// </summary>
        /// <returns></returns>
        public String FileName
        {
            get { return fileName; }
            set
            {
                if (fileName != value && File.Exists (value))
                {
                    lines.Clear ();
                    using (StreamReader sr = new StreamReader (value))
                        while (!sr.EndOfStream)
                            lines.Add (sr.ReadLine ().TrimEnd ());
                }
                fileName = value;
            }
        }

        /// <summary>
        /// Gets the directory.
        /// </summary>
        /// <value>The directory.</value>
        public String Directory
        {
            get { return Path.GetDirectoryName (fileName); }
        }

        #endregion

        #region Value methods (8)

        /// <summary>
        /// Sucht die Zeilennummer (nullbasiert) 
        /// eines gewünschten Eintrages
        /// </summary>
        /// <param name="Caption">Name des Bereiches</param>
        /// <param name="CaseSensitive">true = Gross-/Kleinschreibung beachten</param>
        /// <returns>Nummer der Zeile, sonst -1</returns>
        private int SearchCaptionLine (String Caption, Boolean CaseSensitive)
        {
            if (!CaseSensitive)
                Caption = Caption.ToLower ();
            for (int i = 0; i < lines.Count; i++)
            {
                String line = lines [i].Trim ();
                if (line == "")
                    continue;
                if (!CaseSensitive)
                    line = line.ToLower ();
                // Erst den gewünschten Abschnitt suchen
                if (line == "[" + Caption + "]")
                    return i;
            }
            return -1;// Bereich nicht gefunden
        }

        /// <summary>
        /// Sucht die Zeilennummer (nullbasiert) 
        /// eines gewünschten Eintrages
        /// </summary>
        /// <param name="Caption">Name des Bereiches</param>
        /// <param name="Entry">Name des Eintrages</param>
        /// <param name="CaseSensitive">true = Gross-/Kleinschreibung beachten</param>
        /// <returns>Nummer der Zeile, sonst -1</returns>
        private int SearchEntryLine (String Caption, String Entry, Boolean CaseSensitive)
        {
            Caption = Caption.ToLower ();
            if (!CaseSensitive)
                Entry = Entry.ToLower ();
            int CaptionStart = SearchCaptionLine (Caption, false);
            if (CaptionStart < 0)
                return -1;
            for (int i = CaptionStart + 1; i < lines.Count; i++)
            {
                String line = lines [i].Trim ();
                if (line == "")
                    continue;
                if (!CaseSensitive)
                    line = line.ToLower ();
                if (line.StartsWith ("["))
                    return -1;// Ende, wenn der nächste Abschnitt beginnt
                if (Regex.IsMatch (line, @"^[ \t]*[" + CommentCharacters + "]"))
                    continue; // Kommentar
                if (line.StartsWith (Entry))
                    return i;// Eintrag gefunden
            }
            return -1;// Eintrag nicht gefunden
        }

        /// <summary>
        /// Kommentiert einen Wert aus
        /// </summary>
        /// <param name="Caption">Name des Bereiches</param>
        /// <param name="Entry">Name des Eintrages</param>
        /// <param name="CaseSensitive">true = Gross-/Kleinschreibung beachten</param>
        /// <returns>true = Eintrag gefunden und auskommentiert</returns>
        public Boolean commentValue (String Caption, String Entry, Boolean CaseSensitive)
        {
            int line = SearchEntryLine (Caption, Entry, CaseSensitive);
            if (line < 0)
                return false;
            lines [line] = CommentCharacters [0] + lines [line];
            return true;
        }

        /// <summary>
        /// Löscht einen Wert
        /// </summary>
        /// <param name="Caption">Name des Bereiches</param>
        /// <param name="Entry">Name des Eintrages</param>
        /// <param name="CaseSensitive">true = Gross-/Kleinschreibung beachten</param>
        /// <returns>true = Eintrag gefunden und gelöscht</returns>
        public Boolean deleteValue (String Caption, String Entry, Boolean CaseSensitive)
        {
            int line = SearchEntryLine (Caption, Entry, CaseSensitive);
            if (line < 0)
                return false;
            lines.RemoveAt (line);
            return true;
        }

        /// <summary>
        /// Liest den Wert eines Eintrages aus
        /// (Erweiterung: case sensitive)
        /// </summary>
        /// <param name="Caption">Name des Bereiches</param>
        /// <param name="Entry">Name des Eintrages</param>
        /// <param name="CaseSensitive">true = Gross-/Kleinschreibung beachten</param>
        /// <returns>Wert des Eintrags oder leer</returns>
        public String getValue (String Caption, String Entry, Boolean CaseSensitive)
        {
            int line = SearchEntryLine (Caption, Entry, CaseSensitive);
            if (line < 0)
                return "";
            int pos = lines [line].IndexOf ("=");
            if (pos < 0)
                return "";
            return lines [line].Substring (pos + 1).Trim ();
            // Evtl. noch abschliessende Kommentarbereiche entfernen
        }

        /// <summary>
        /// Setzt einen Wert in einem Bereich. Wenn der Wert
        /// (und der Bereich) noch nicht existiert, werden die
        /// entsprechenden Einträge erstellt.
        /// </summary>
        /// <param name="Caption">Name des Bereichs</param>
        /// <param name="Entry">name des Eintrags</param>
        /// <param name="Value">Wert des Eintrags</param>
        /// <param name="CaseSensitive">true = Gross-/Kleinschreibung beachten</param>
        /// <returns>true = Eintrag erfolgreich gesetzt</returns>
        public Boolean setValue (String Caption, String Entry, String Value, Boolean CaseSensitive)
        {
            Caption = Caption.ToLower ();
            if (!CaseSensitive)
                Entry = Entry.ToLower ();
            int lastCommentedFound = -1;
            int CaptionStart = SearchCaptionLine (Caption, false);
            if (CaptionStart < 0)
            {
                lines.Add ("[" + Caption + "]");
                lines.Add (Entry + "=" + Value);
                return true;
            }
            int EntryLine = SearchEntryLine (Caption, Entry, CaseSensitive);
            for (int i = CaptionStart + 1; i < lines.Count; i++)
            {
                String line = lines [i].Trim ();
                if (!CaseSensitive)
                    line = line.ToLower ();
                if (line == "")
                    continue;
                // Ende, wenn der nächste Abschnitt beginnt
                if (line.StartsWith ("["))
                {
                    lines.Insert (i, Entry + "=" + Value);
                    return true;
                }
                // Suche aukommentierte, aber gesuchte Einträge
                // (evtl. per Parameter bestimmen können?), falls
                // der Eintrag noch nicht existiert.
                if (EntryLine < 0)
                    if (Regex.IsMatch (line, @"^[ \t]*[" + CommentCharacters + "]"))
                    {
                        String tmpLine = line.Substring (1).Trim ();
                        if (tmpLine.StartsWith (Entry))
                        {
                            // Werte vergleichen, wenn gleich,
                            // nur Kommentarzeichen löschen
                            int pos = tmpLine.IndexOf ("=");
                            if (pos > 0)
                            {
                                if (Value == tmpLine.Substring (pos + 1).Trim ())
                                {
                                    lines [i] = tmpLine;
                                    return true;
                                }
                            }
                            lastCommentedFound = i;
                        }
                        continue;// Kommentar
                    }
                if (line.StartsWith (Entry))
                {
                    lines [i] = Entry + "=" + Value;
                    return true;
                }
            }
            if (lastCommentedFound > 0)
                lines.Insert (lastCommentedFound + 1, Entry + "=" + Value);
            else
                lines.Insert (CaptionStart + 1, Entry + "=" + Value);
            return true;
        }

        /// <summary>
        /// Liest alle Einträge uns deren Werte eines Bereiches aus
        /// </summary>
        /// <param name="Caption">Name des Bereichs</param>
        /// <returns>Sortierte Liste mit Einträgen und Werten</returns>
        public SortedList<String, String> getCaption (String Caption)
        {
            SortedList<String, String> result = new SortedList<string, string> ();
            Boolean CaptionFound = false;
            for (int i = 0; i < lines.Count; i++)
            {
                String line = lines [i].Trim ();
                if (line == "")
                    continue;
                // Erst den gewünschten Abschnitt suchen
                if (!CaptionFound)
                    if (line.ToLower () != "[" + Caption + "]")
                        continue;
                    else
                    {
                        CaptionFound = true;
                        continue;
                    }
                // Ende, wenn der nächste Abschnitt beginnt
                if (line.StartsWith ("["))
                    break;
                if (Regex.IsMatch (line, @"^[ \t]*[" + CommentCharacters + "]"))
                    continue; // Kommentar
                int pos = line.IndexOf ("=");
                if (pos < 0)
                    result.Add (line, "");
                else
                    result.Add (line.Substring (0, pos).Trim (), line.Substring (pos + 1).Trim ());
            }
            return result;
        }

        /// <summary>
        /// Erstellt eine Liste aller enthaltenen Bereiche
        /// </summary>
        /// <returns>Liste mit gefundenen Bereichen</returns>
        public List<string> getAllCaptions ()
        {
            List<string> result = new List<string> ();
            for (int i = 0; i < lines.Count; i++)
            {
                String line = lines [i];
                Match mCaption = regCaption.Match (lines [i]);
                if (mCaption.Success)
                    result.Add (mCaption.Groups ["caption"].Value.Trim ());
            }
            return result;
        }

        #endregion

        #region Ini file methods (2)

        /// <summary>
        /// Datei sichern
        /// </summary>
        /// <returns></returns>
        public Boolean Save ()
        {
            if (string.IsNullOrWhiteSpace (fileName))
                return false;

            try
            {
                using (StreamWriter sw = new StreamWriter (fileName))
                    foreach (String line in lines)
                        sw.WriteLine (line);
            }
            #region StreamWriter_Exceptions
            catch (UnauthorizedAccessException excp)
            {
                Debug.WriteLine ("Fehler beim starten des TMC", "UnauthorizedAccessException");
                Debug.WriteLine (excp.Message, "UnauthorizedAccessException");
            }
            catch (ArgumentException excp)
            {
                Debug.WriteLine ("Fehler beim starten des TMC", "ArgumentException");
                Debug.WriteLine (excp.Message, "ArgumentException");
            }
            catch (DirectoryNotFoundException excp)
            {
                Debug.WriteLine ("Fehler beim starten des TMC", "DirectoryNotFoundException");
                Debug.WriteLine (excp.Message, "DirectoryNotFoundException");
            }
            catch (PathTooLongException excp)
            {
                Debug.WriteLine ("Fehler beim starten des TMC", "PathTooLongException");
                Debug.WriteLine (excp.Message, "PathTooLongException");
            }
            catch (IOException excp)
            {
                Debug.WriteLine ("Fehler beim starten des TMC", "IOException");
                Debug.WriteLine (excp.Message, "IOException");
            }
            catch (SecurityException excp)
            {
                Debug.WriteLine ("Fehler beim starten des TMC", "SecurityException");
                Debug.WriteLine (excp.Message, "SecurityException");
            }
            catch (Exception excp)
            {
                Debug.WriteLine ("Fehler beim starten des TMC", "Unknown_Exception");
                Debug.WriteLine (excp.Message, "Unknown_Exception");
            }
            #endregion
            return true;
        }

        /// <summary>
        /// Datei unter neuem Namen speichern sichern
        /// </summary>
        /// <param name="newFileName">[Pfad]Name der neuen INI Datei</param>
        /// <param name="overWrite">Überschreiben einer bestehenden Datei. Default=True</param>
        /// <returns>True bei erfolg</returns>
        public Boolean SaveAs (string newFileName, bool overWrite = true)
        {
            if (string.IsNullOrWhiteSpace (fileName))
                return false;

            if (!overWrite && File.Exists (newFileName))
            {
                return false;
            }
            try
            {
                using (StreamWriter sw = new StreamWriter (fileName))
                    foreach (String line in lines)
                        sw.WriteLine (line);
            }
            #region StreamWriter_Exceptions
            catch (UnauthorizedAccessException excp)
            {
                Debug.WriteLine ("Fehler beim starten des TMC", "UnauthorizedAccessException");
                Debug.WriteLine (excp.Message, "UnauthorizedAccessException");
            }
            catch (ArgumentException excp)
            {
                Debug.WriteLine ("Fehler beim starten des TMC", "ArgumentException");
                Debug.WriteLine (excp.Message, "ArgumentException");
            }
            catch (DirectoryNotFoundException excp)
            {
                Debug.WriteLine ("Fehler beim starten des TMC", "DirectoryNotFoundException");
                Debug.WriteLine (excp.Message, "DirectoryNotFoundException");
            }
            catch (PathTooLongException excp)
            {
                Debug.WriteLine ("Fehler beim starten des TMC", "PathTooLongException");
                Debug.WriteLine (excp.Message, "PathTooLongException");
            }
            catch (IOException excp)
            {
                Debug.WriteLine ("Fehler beim starten des TMC", "IOException");
                Debug.WriteLine (excp.Message, "IOException");
            }
            catch (SecurityException excp)
            {
                Debug.WriteLine ("Fehler beim starten des TMC", "SecurityException");
                Debug.WriteLine (excp.Message, "SecurityException");
            }
            catch (Exception excp)
            {
                Debug.WriteLine ("Fehler beim starten des TMC", "Unknown_Exception");
                Debug.WriteLine (excp.Message, "Unknown_Exception");
            }
            #endregion

            return true;
        }

        /// <summary>
        /// Exportiert sämtliche Bereiche und deren Werte
        /// in ein XML-Dokument
        /// </summary>
        /// <returns>XML-Dokument</returns>
        public XmlDocument exportToXml ()
        {
            XmlDocument doc = new XmlDocument ();
            XmlElement root = doc.CreateElement (
                Path.GetFileNameWithoutExtension (this.fileName));
            doc.AppendChild (root);
            XmlElement Caption = null;
            for (int i = 0; i < lines.Count; i++)
            {
                Match mEntry = regEntry.Match (lines [i]);
                Match mCaption = regCaption.Match (lines [i]);
                if (mCaption.Success)
                {
                    Caption = doc.CreateElement (mCaption.Groups ["caption"].Value.Trim ());
                    root.AppendChild (Caption);
                    continue;
                }
                if (mEntry.Success)
                {
                    XmlElement xe = doc.CreateElement (mEntry.Groups ["entry"].Value.Trim ());
                    xe.InnerXml = mEntry.Groups ["value"].Value.Trim ();
                    if (Caption == null)
                        root.AppendChild (xe);
                    else
                        Caption.AppendChild (xe);
                }
            }
            return doc;
        }

        #endregion
    }

    /// <summary>
    /// Klasse zum schreiben einer Logdatei
    /// </summary>
    public static class Logger
    {
        static private object _objOutput;

        static public object objOutput
        {
            get { return _objOutput; }
            set { _objOutput = value; }
        }

        public static void Error (string message, string module)
        {
            WriteEntry (message, "error", module);
        }

        public static void Error (Exception ex, string module)
        {
            WriteEntry (ex.Message, "error", module);
        }

        public static void Warning (string message, string module)
        {
            WriteEntry (message, "warning", module);
        }

        public static void Info (string message, string module)
        {
            WriteEntry (message, "info", module);
        }

        private static void WriteEntry (string message, string type, string module)
        {
            string text2output = string.Format ("{0} | {1} | {2} | {3}",
                                  DateTime.Now.ToString ("dd-MM-yyyy HH:mm:ss"),
                                  type,
                                  module,
                                  message);
            //	    	if (objOutput != null)
            //	    		writeToAppLog(text2output);

            Trace.WriteLine (text2output);
        }

        private static void writeToAppLog (string text)
        {
            if (!string.IsNullOrWhiteSpace (text))
            {
                if (objOutput != null)
                {
                    if (objOutput.GetType () == typeof (RichTextBox))
                    {
                        (objOutput as RichTextBox).AppendText (text);
                    }
                }
            }
        }
    }
}
