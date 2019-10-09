//Level.cs

/* Developed for Intel(R) Perceptual Computing Challenge 2013
 * by Prabu Arumugam (cincoutprabu@gmail.com)
 * http://www.codeding.com/
 * 2013-Aug-16
*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;

namespace PerceptualPegSolitaire.Entities
{
    public class Level
    {
        #region Properties

        public int Number { get; set; }
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public bool Unlocked { get; set; }
        public int BestPebbleCount { get; set; }

        public bool TaglineVisible
        {
            get
            {
                return Unlocked && BestPebbleCount > 0;
            }
        }

        #endregion

        #region Constructors

        public Level()
        {
            this.Number = 0;
            this.Name = string.Empty;
            this.ImagePath = string.Empty;
            Unlocked = false;
            this.BestPebbleCount = 0;
        }

        #endregion

        #region Methods

        public Board GetBoard()
        {
            return Board.BuildBoard(AllLevels.BoardList[this.Name]);
        }

        public XmlNode ToXml(XmlDocument doc)
        {
            XmlElement node = doc.CreateElement("Level");
            node.Attributes.Append(CreateAttribute(doc, "Number", this.Number.ToString()));
            node.Attributes.Append(CreateAttribute(doc, "Name", this.Name));
            node.Attributes.Append(CreateAttribute(doc, "Unlocked", this.Unlocked.ToString()));
            node.Attributes.Append(CreateAttribute(doc, "Best", this.BestPebbleCount.ToString()));
            return node;
        }

        #endregion

        #region Helper-Methods

        public static Level FromXml(XmlNode node)
        {
            var level = new Level();
            level.Number = int.Parse(node.Attributes["Number"].Value);
            level.Name = node.Attributes["Name"].Value;
            level.ImagePath = string.Format("Images/Levels/{0}.png", level.Name);
            level.Unlocked = Convert.ToBoolean(node.Attributes["Unlocked"].Value);
            level.BestPebbleCount = int.Parse(node.Attributes["Best"].Value);
            return level;
        }

        public static XmlAttribute CreateAttribute(XmlDocument doc, string name, string value)
        {
            XmlAttribute attribute = doc.CreateAttribute(name);
            attribute.Value = value;
            return attribute;
        }

        #endregion
    }

    public static class AllLevels
    {
        public static string[] Blank = new string[]
        {
            "       ",
            "       ",
            "       ",
            "       ",
            "       ",
            "       ",
            "       ",
        };

        public static string[] Home = new string[]
        {
            "   1   ",
            "  111  ",
            " 11111 ",
            "1111111",
            " 11111 ",
            " 10001 ",
            " 10001 ",
        };

        public static string[] Box = new string[]
        {
            "       ",
            " 11111 ",
            " 11111 ",
            " 11011 ",
            " 11111 ",
            " 11111 ",
            "       ",
        };

        public static string[] Circle = new string[]
        {
            "   1   ",
            "  111  ",
            " 11011 ",
            "1100011",
            " 11011 ",
            "  111  ",
            "   1   ",
        };

        public static string[] Tree = new string[]
        {
            "  111  ",
            "1111111",
            " 11011 ",
            "  101  ",
            "  101  ",
            "  111  ",
            "  111  ",
        };

        public static string[] Arrow = new string[]
        {
            "   1   ",
            "  111  ",
            " 11011 ",
            " 11111 ",
            "  111  ",
            "  111  ",
            "  111  ",
        };

        public static string[] Cross = new string[]
        {
            "  111  ",
            "  111  ",
            " 11011 ",
            " 10001 ",
            " 11011 ",
            "  111  ",
            "  111  ",
        };

        public static string[] Checkers = new string[]
        {
            " 11 11 ",
            "1111111",
            "1110111",
            " 10001 ",
            "1110111",
            "1111111",
            " 11 11 ",
        };

        public static string[] Island = new string[]
        {
            "  111  ",
            "  111  ",
            "1100011",
            "1100011",
            "1100011",
            "  111  ",
            "  111  ",
        };

        public static string[] Solitaire = new string[]
        {
            "  111  ",
            " 11111 ",
            "1111111",
            "1110111",
            "1111111",
            " 11111 ",
            "  111  ",
        };

        public static Dictionary<string, string[]> BoardList = new Dictionary<string, string[]>()
        {
            {"Arrow", AllLevels.Arrow},
            {"Cross", AllLevels.Cross},
            {"Box", AllLevels.Box},
            {"Home", AllLevels.Home},
            {"Tree", AllLevels.Tree},
            {"Checkers", AllLevels.Checkers},
            {"Circle", AllLevels.Circle},
            {"Island", AllLevels.Island},
            {"Solitaire", AllLevels.Solitaire}
        };

        public static List<Level> LevelList = new List<Level>();
        public static bool LoadLevels()
        {
            LevelList.Clear();

            try
            {
                string dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.DATA_FILENAME);
                if (!File.Exists(dataPath))
                {
                    var assemblyPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Constants.DATA_FILENAME);
                    File.Copy(assemblyPath, dataPath, true);
                }

                if (File.Exists(dataPath))
                {
                    //read player-data-xml
                    XmlDocument doc = new XmlDocument();
                    doc.Load(dataPath);

                    var levelNodes = doc.SelectNodes("Player/Level");
                    foreach (XmlNode node in levelNodes)
                    {
                        var level = Level.FromXml(node);
                        LevelList.Add(level);
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static void SaveLevels()
        {
            try
            {
                string dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.DATA_FILENAME);
                if (File.Exists(dataPath))
                {
                    //save player-data to xml
                    XmlDocument doc = new XmlDocument();
                    XmlElement root = doc.CreateElement("Player");
                    foreach (Level level in LevelList)
                    {
                        var levelNode = level.ToXml(doc);
                        root.AppendChild(levelNode);
                    }
                    doc.AppendChild(root);

                    doc.Save(dataPath);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
