﻿using ClassicUO.Utility;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClassicUO.IO;

namespace ClassicUO.Game.Managers
{
    static class SkillsGroupManager
    {
        public static Dictionary<string, List<int>> Groups { get; } = new Dictionary<string, List<int>>();

        private static void MakeCUODefault()
        {
            Groups.Clear();

            int count = FileManager.Skills.SkillsCount;

            Groups.Add("Miscellaneous", new List<int>()
                {
                    4, 6, 10, 12, 19, 3, 36
                }
            );

            Groups.Add("Combat", new List<int>()
                {
                    1, 31, 42, 17, 41, 5, 40, 27
                }
            );

            if (count > 57)
                Groups["Combat"].Add(57);
            Groups["Combat"].Add(43);
            if (count > 50)
                Groups["Combat"].Add(50);
            if (count > 51)
                Groups["Combat"].Add(51);
            if (count > 52)
                Groups["Combat"].Add(52);
            if (count > 53)
                Groups["Combat"].Add(53);


            Groups.Add("Trade Skills", new List<int>()
                {
                    0, 7, 8, 11, 13, 23, 44, 45, 34, 37
                }
            );

            Groups.Add("Magic", new List<int>()
            {
                16
            });

            if (count > 56)
                Groups["Magic"].Add(56);
            Groups["Magic"].Add(25);
            Groups["Magic"].Add(46);
            if (count > 55)
                Groups["Magic"].Add(55);
            Groups["Magic"].Add(26);
            if (count > 54)
                Groups["Magic"].Add(54);
            Groups["Magic"].Add(32);
            if (count > 49)
                Groups["Magic"].Add(49);


            Groups.Add("Wilderness", new List<int>()
                {
                    2, 35, 18, 20, 38, 39
                }
            );

            Groups.Add("Thieving", new List<int>()
                {
                    14, 21, 24, 30, 48, 28, 33, 47
                }
            );

            Groups.Add("Bard", new List<int>()
                {
                    15, 29, 9, 22
                }
            );
        }

        public static void MakeDefault()
        {
            FileInfo info = new FileInfo(Path.Combine(FileManager.UoFolderPath, "skillgrp.mul"));
            try
            {
                if (!info.Exists)
                {
                    MakeCUODefault();
                    return;
                }
                Groups.Clear();
                using (FileStream fs = new FileStream(info.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    int skillidx = 0;
                    bool unicode = false;
                    using (BinaryReader bin = new BinaryReader(fs))
                    {
                        int start = 4;
                        int strlen = 17;
                        int count = bin.ReadInt32();
                        if (count == -1)
                        {
                            unicode = true;
                            count = bin.ReadInt32();
                            start *= 2;
                            strlen *= 2;
                        }

                        List<string> groups = new List<string>();
                        groups.Add("Miscellaneous");
                        Groups.Add("Miscellaneous", new List<int>());
                        for (int i = 0; i < count - 1; ++i)
                        {
                            int strbuild;
                            fs.Seek((long)(start + (i * strlen)), SeekOrigin.Begin);
                            StringBuilder sb = new StringBuilder(17);
                            if (unicode)
                            {
                                while ((strbuild = bin.ReadInt16()) != 0)
                                    sb.Append((char)strbuild);
                            }
                            else
                            {
                                while ((strbuild = bin.ReadByte()) != 0)
                                    sb.Append((char)strbuild);
                            }
                            groups.Add(sb.ToString());
                            Groups.Add(sb.ToString(), new List<int>());
                        }
                        fs.Seek((long)(start + ((count - 1) * strlen)), SeekOrigin.Begin);
                        while (bin.BaseStream.Length != bin.BaseStream.Position)
                        {
                            int grp = bin.ReadInt32();
                            if(grp < groups.Count)
                                Groups[groups[grp]].Add(skillidx++);
                        }
                    }
                }
            }
            catch
            {
                MakeCUODefault();
            }
        }


        public static bool AddNewGroup(string group)
        {
            if (!Groups.ContainsKey(group))
            {
                Groups.Add(group, new List<int>());

                return true;
            }

            return false;
        }

        public static void RemoveGroup(string group)
        {
            if (Groups.TryGetValue(group, out var list))
            {
                Groups.Remove(group);

                if (Groups.Count == 0)
                {
                    Groups.Add("All", list);
                }
                else
                {
                    Groups.FirstOrDefault().Value.AddRange(list);
                }
            }
        }

        public static List<int> GetSkillsInGroup(string group)
        {
            Groups.TryGetValue(group, out var list);

            return list;
        }

        public static void ReplaceGroup(string oldGroup, string newGroup)
        {
            if (Groups.TryGetValue(oldGroup, out var oldList) && !Groups.TryGetValue(newGroup, out var newList))
            {
                Groups.Remove(oldGroup);
                Groups[newGroup] = oldList;
            }
        }

        public static void MoveSkillToGroup(string oldGroup, string newGroup, int skillIndex)
        {
            if (Groups.TryGetValue(oldGroup, out var oldList) && Groups.TryGetValue(newGroup, out var newList))
            {
                oldList.Remove(skillIndex);
                newList.Add(skillIndex);
            }
        }

        public static void Load(BinaryReader reader)
        {
            Groups.Clear();

            int version = reader.ReadInt32();

            int groupCount = reader.ReadInt32();

            for (int i = 0; i < groupCount; i++)
            {
                int entriesCount = reader.ReadInt32();
                string groupName = reader.ReadUTF8String(reader.ReadInt32());

                if (!Groups.TryGetValue(groupName, out var list) || list == null)
                {
                    list = new List<int>();
                    Groups[groupName] = list;
                }

                for (int j = 0; j < entriesCount; j++)
                {
                    int skillIndex = reader.ReadInt32();
                    list.Add(skillIndex);
                }
            }
        }

        public static void Save(BinaryWriter writer)
        {
            // version
            writer.Write(1);

            writer.Write(Groups.Count);

            foreach (KeyValuePair<string, List<int>> k in Groups)
            {
                writer.Write(k.Value.Count);

                writer.Write(k.Key.Length);
                writer.WriteUTF8String(k.Key);
                foreach (int i in k.Value)
                {
                    writer.Write(i);
                }
            }
        }
    }
}
