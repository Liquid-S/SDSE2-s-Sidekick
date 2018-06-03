using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SDSE2_s_Sidekick
{
    public partial class Sidekick : Form
    {
        public Sidekick()
        {
            InitializeComponent();

            //Read Data01's Path from "Config.txt"
            if (File.Exists("Config.txt") == true)
                LoadPath();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Liquid-S");
        }

        // AlphanumComparatorFast taken from https://gist.github.com/ngbrown/3842065
        public class AlphanumComparatorFast : System.Collections.IComparer
        {
            public int Compare(object x, object y)
            {
                string s1 = x as string;
                if (s1 == null)
                    return 0;

                string s2 = y as string;
                if (s2 == null)
                    return 0;

                int len1 = s1.Length, len2 = s2.Length;
                int marker1 = 0, marker2 = 0;

                // Walk through two the strings with two markers.
                while (marker1 < len1 && marker2 < len2)
                {
                    char ch1 = s1[marker1], ch2 = s2[marker2];

                    // Some buffers we can build up characters in for each chunk.
                    char[] space1 = new char[len1], space2 = new char[len2];
                    int loc1 = 0, loc2 = 0;

                    // Walk through all following characters that are digits or
                    // characters in BOTH strings starting at the appropriate marker.
                    // Collect char arrays.
                    do
                    {
                        space1[loc1++] = ch1;
                        marker1++;

                        if (marker1 < len1)
                            ch1 = s1[marker1];
                        else
                            break;

                    } while (char.IsDigit(ch1) == char.IsDigit(space1[0]));

                    do
                    {
                        space2[loc2++] = ch2;
                        marker2++;

                        if (marker2 < len2)
                            ch2 = s2[marker2];
                        else
                            break;

                    } while (char.IsDigit(ch2) == char.IsDigit(space2[0]));

                    // If we have collected numbers, compare them numerically.
                    // Otherwise, if we have strings, compare them alphabetically.
                    string str1 = new string(space1), str2 = new string(space2);

                    int result;

                    if (char.IsDigit(space1[0]) && char.IsDigit(space2[0]))
                    {
                        int thisNumericChunk = int.Parse(str1);
                        int thatNumericChunk = int.Parse(str2);
                        result = thisNumericChunk.CompareTo(thatNumericChunk);
                    }
                    else
                        result = str1.CompareTo(str2);


                    if (result != 0)
                        return result;

                }
                return len1 - len2;
            }
        }

        // Clone the directories, this way the tool can delete, change and repack everything without worrying about damage the user work.
        private void CloneDirectory(string OriginalDir, string TEMPFolder)
        {
            DirectoryInfo source = new DirectoryInfo(OriginalDir),
                target = new DirectoryInfo(TEMPFolder);

            // Delte the TEMPDir if it already exist.
            if (Directory.Exists(target.FullName) == true)
            {
                Directory.Delete(target.FullName, true);
                while (Directory.Exists(target.FullName)) { }
            }

            // Create the TEMPDir and make it invisible.
            DirectoryInfo NewTEMPDir = Directory.CreateDirectory(target.FullName);
            NewTEMPDir.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

            string[] extensions = null;

            extensions = new[] { ".lin", ".pak", ".txt", ".scp.wrd" };

            // Copy the files to the TEMP folder. Images are converted only if the user has requested.
            foreach (FileInfo fi in source.EnumerateFiles().Where(f => extensions.Contains(f.Extension.ToLower())).ToArray())
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);

            // Copy the subfolders and their contents.
            foreach (string SubDir in Directory.GetDirectories(OriginalDir, "*", SearchOption.TopDirectoryOnly))
                CloneDirectory(SubDir, Path.Combine(TEMPFolder, Path.GetFileName(SubDir)));
        }

        private void LoadPath() // Read Data01's Path from "Config.txt".
        {
            using (FileStream ConfigTXT = new FileStream("Config.txt", FileMode.Open, FileAccess.Read))
            using (StreamReader DP = new StreamReader(ConfigTXT, Encoding.Default))
                textBox1.Text = DP.ReadLine();
        }

        private void SetDATA01Path() // Save Data01's Path in the textbox and into "Config.txt".
        {
            FolderBrowserDialog Data01Path = new FolderBrowserDialog();

            if (Data01Path.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = Data01Path.SelectedPath; // Write the Path inside the textbox.

                using (FileStream ConfigTXT = new FileStream("Config.txt", FileMode.Create, FileAccess.Write))
                using (StreamWriter DP = new StreamWriter(ConfigTXT, Encoding.Default))
                    DP.WriteLine(Data01Path.SelectedPath);
            }
        }

        private void Button1_Click(object sender, EventArgs e) // "Set DATA01's Path..."
        {
            SetDATA01Path();
        }

        private void button3_Click(object sender, EventArgs e) // BUILD TEXT FILES
        {
            if (textBox1.Text == "" || textBox1.Text == null || textBox1.Text.Contains("Click on \"Set ")) //BUILD TEXT FILES
                SetDATA01Path();

            label2.Text = "Wait..."; // Change "Ready!" to "Wait..."
            label2.Refresh(); // Refresh the Status label.

            string ScriptDir = Path.Combine(textBox1.Text, "jp\\script");

            if (Directory.Exists(ScriptDir) && Directory.EnumerateDirectories(ScriptDir).Any() == true)
            {
                string RepackedScriptDestination = "REPACKED TEXT\\dr2_data_keyboard_us\\Dr2\\data\\us\\script";

                if (Directory.Exists(RepackedScriptDestination) == true)
                {
                    Directory.Delete(RepackedScriptDestination, true);
                    while (Directory.Exists(RepackedScriptDestination)) { }
                }
                Directory.CreateDirectory(RepackedScriptDestination);

                foreach (string DirToBeRepacked in Directory.GetDirectories(ScriptDir, "*", SearchOption.TopDirectoryOnly))
                    if (Directory.GetFiles(DirToBeRepacked, "*.txt").Length > 0) //If positive, then the the DirToBeRepacked == ".lin" || .pak Type 1
                        RePackText(DirToBeRepacked, RepackedScriptDestination);

                    else if (Directory.EnumerateDirectories(DirToBeRepacked).Any() == true) //else Dir == .pak Type 2
                    {
                        string TEMPDir = Path.GetFileNameWithoutExtension(DirToBeRepacked) + Path.GetExtension(DirToBeRepacked);

                        if (Directory.Exists(TEMPDir) == true)
                        {
                            Directory.Delete(TEMPDir, true);
                            while (Directory.Exists(TEMPDir)) { }
                        }

                        Directory.CreateDirectory(TEMPDir);

                        foreach (string SubDirs in Directory.GetDirectories(DirToBeRepacked, "*", SearchOption.TopDirectoryOnly))
                            RePackText(SubDirs, TEMPDir);

                        RePackPAK(TEMPDir, RepackedScriptDestination, 0);

                        // Delete the TEMPDir. We don't need it anymore.
                        Directory.Delete(TEMPDir, true);
                        while (Directory.Exists(TEMPDir)) { }
                    }
            }

            string JPBinDIr = Path.Combine(textBox1.Text, "jp\\bin");

            if (Directory.Exists(JPBinDIr) && Directory.EnumerateDirectories(JPBinDIr).Any() == true)
            {
                string RepackedJPBinDestination = "REPACKED TEXT\\dr2_data_keyboard_us\\Dr2\\data\\us\\bin";

                if (Directory.Exists(RepackedJPBinDestination) == true)
                {
                    Directory.Delete(RepackedJPBinDestination, true);
                    while (Directory.Exists(RepackedJPBinDestination)) { }
                }
                Directory.CreateDirectory(RepackedJPBinDestination);

                // Copy every file.DAT from "\\jp\\bin" to the DestinationDir.
                foreach (string DAT in Directory.GetFiles(JPBinDIr, "*.dat", SearchOption.TopDirectoryOnly))
                    File.Copy(DAT, Path.Combine(RepackedJPBinDestination, Path.GetFileNameWithoutExtension(DAT) + ".dat"), true);

                foreach (string DirToBeRepacked in Directory.GetDirectories(JPBinDIr, "*", SearchOption.TopDirectoryOnly))
                    if (Directory.EnumerateDirectories(DirToBeRepacked).Any() == true)
                    {
                        string TEMPDir1 = Path.GetFileNameWithoutExtension(DirToBeRepacked) + Path.GetExtension(DirToBeRepacked);

                        if (Directory.Exists(TEMPDir1) == true)
                        {
                            Directory.Delete(TEMPDir1, true);
                            while (Directory.Exists(TEMPDir1)) { }
                        }

                        Directory.CreateDirectory(TEMPDir1);

                        foreach (string SubDirs in Directory.GetDirectories(DirToBeRepacked, "*", SearchOption.TopDirectoryOnly))
                            RePackText(SubDirs, TEMPDir1);

                        RePackPAK(TEMPDir1, RepackedJPBinDestination, 0);

                        // Delete the TEMPDir. We don't need it anymore.
                        Directory.Delete(TEMPDir1, true);
                        while (Directory.Exists(TEMPDir1)) { }

                    }
            }


            // START - Copy every file.DAT from "\\all\\bin" to the DestinationDir.
            string ALLBinDIr = Path.Combine(textBox1.Text, "all\\bin");

            if (Directory.Exists(ALLBinDIr) && Directory.EnumerateDirectories(ALLBinDIr).Any() == true)
            {
                string RepackedALLBinDestination = "REPACKED TEXT\\dr2_data_keyboard_us\\Dr2\\data\\all\\bin";

                if (Directory.Exists(RepackedALLBinDestination) == true)
                {
                    Directory.Delete(RepackedALLBinDestination, true);
                    while (Directory.Exists(RepackedALLBinDestination)) { }
                }
                Directory.CreateDirectory(RepackedALLBinDestination);

                // Copy every file.DAT from "\\jp\\bin" to the DestinationDir.
                foreach (string DAT in Directory.GetFiles(ALLBinDIr, "*.dat", SearchOption.TopDirectoryOnly))
                    File.Copy(DAT, Path.Combine(RepackedALLBinDestination, Path.GetFileNameWithoutExtension(DAT) + ".dat"), true);
            }
            // END - Copy every file.DAT from "\\all\\bin" to the DestinationDir.

            label2.Text = "Ready!"; // Change the "Status" to "Ready!".
            MessageBox.Show("Done!", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void RePackText(string DirTextToBeRepacked, string DestinationDir)
        {
            string NewFileExtension = ".pak",
            BytecodeAddress = Path.Combine(DirTextToBeRepacked, Path.GetFileNameWithoutExtension(DirTextToBeRepacked) + ".scp.wrd");

            // If there is a ".scp.wrd" file, it means it's a ".LIN" file.
            if (File.Exists(BytecodeAddress) == true)
            {
                NewFileExtension = ".lin";

                // The SDSE2 uses the bytecodes from the PSP version, so we need to change them for the Steam version. */
                if (DirTextToBeRepacked.Contains("novel") && File.Exists(Path.Combine("ByteCodeFix", Path.GetFileNameWithoutExtension(DirTextToBeRepacked) + ".bytecode")) == true)
                    BytecodeAddress = Path.Combine("ByteCodeFix", Path.GetFileNameWithoutExtension(DirTextToBeRepacked) + ".bytecode");
            }

            List<string> TranslatedSentences = new List<string>();

            uint NParts = 0x02,
                HeaderSize = 0x10;

            // It extracts all the phrases translated from the TXTs and stores them in "TranslatedSentences".
            foreach (string FileTXT in Directory.GetFiles(DirTextToBeRepacked, "*.txt", SearchOption.TopDirectoryOnly))
                using (FileStream TranslatedTXT = new FileStream(FileTXT, FileMode.Open, FileAccess.Read))
                using (StreamReader TXTStreamReader = new StreamReader(TranslatedTXT, Encoding.UTF8))
                    TranslatedSentences.Add(TextBetweenTAGsReader(TXTStreamReader.ReadToEnd(), DirTextToBeRepacked.Replace(textBox1.Text, null)));

            if (TranslatedSentences == null)
            {
                NParts = 0x01;
                HeaderSize = 0x0C;
            }

            using (FileStream REPACKEDFILE = new FileStream(Path.Combine(DestinationDir, Path.GetFileNameWithoutExtension(DirTextToBeRepacked) + NewFileExtension), FileMode.Create, FileAccess.Write))
            using (BinaryWriter LINBinaryWriter = new BinaryWriter(REPACKEDFILE), LINBinUnicode = new BinaryWriter(REPACKEDFILE, Encoding.Unicode))
            {
                int FileSizePos = 0;

                // START INSERTING BYCODE - ONLY IF IT'S A LIN.
                if (NewFileExtension == ".lin") // If it's a "LIN" that means there is a ".scp.wrd" file.
                {
                    LINBinaryWriter.Write(NParts);
                    LINBinaryWriter.Write(HeaderSize);

                    using (FileStream BYTECODE = new FileStream(BytecodeAddress, FileMode.Open, FileAccess.Read))
                    {
                        /* If the header sizes is equal to 0x10 that means that are two files
                        and therefore we must write the offset of the second file. */
                        if (HeaderSize == 0x10)
                            LINBinaryWriter.Write((uint)(HeaderSize + BYTECODE.Length));

                        FileSizePos = (int)REPACKEDFILE.Position;
                        LINBinaryWriter.Write((uint)(0x0));
                        BYTECODE.CopyTo(REPACKEDFILE, (int)HeaderSize);
                    }
                }
                // END INSERTING BYCODE - ONLY IF IT'S A LIN.

                // START INSERTING TEXT - Only if there is text to be processed.
                if (TranslatedSentences != null)
                {
                    // "SentencesOffset" will contain the offset of each phrase.
                    uint[] SentencesOffset = new uint[TranslatedSentences.Count + 1];
                    byte Padding = 0x02; // DR2 padding is 2.

                    // Write down the n# of sentences.
                    LINBinaryWriter.Write((uint)TranslatedSentences.Count);

                    // Stores the current position so that we can come back later and enter the correct offsets.
                    int pos = (int)REPACKEDFILE.Position;

                    // Fills the pointers area with zeros. At the end of the process the area will be overwritten with the correct data.
                    for (int i = 0; i < SentencesOffset.Length; i++)
                        LINBinaryWriter.Write((uint)0x00);

                    /* The "- ((uint) pos - 4)" is due to the fact that the offsets do not take into account everything that
                    is before the number of sentences, ergo the bytecode and the first 0x10. */
                    SentencesOffset[0] = (uint)REPACKEDFILE.Position - ((uint)pos - 4);

                    for (int i = 0; i < TranslatedSentences.Count; i++)
                    {
                        LINBinaryWriter.Write((ushort)0xFEFF); //BOM

                        // Write the sentence n# [i] in the repacked file.
                        LINBinUnicode.Write(TranslatedSentences[i].ToCharArray());

                        // Write down the null string terminator.
                        LINBinaryWriter.Write((ushort)0x00);

                        // If it's a "PAK" that means that we must padding.
                        if (NewFileExtension == ".pak")
                        {
                            if (REPACKEDFILE.Position % 0x04 != 0)
                                while (REPACKEDFILE.Position % 0x04 != 0)
                                    LINBinaryWriter.Write((byte)0x0);

                            SentencesOffset[i + 1] = (uint)REPACKEDFILE.Position;
                        }
                        else
                            SentencesOffset[i + 1] = (uint)REPACKEDFILE.Position - ((uint)pos - 4);
                    }

                    // Padding at the end of the file.
                    if (REPACKEDFILE.Position % Padding != 0)
                        while (REPACKEDFILE.Position % Padding != 0)
                            LINBinaryWriter.Write((byte)0x0);

                    // Comes back in the area dedicated to the offsets and overwrites all the zeros with the correct offsets.
                    LINBinaryWriter.Seek(pos, SeekOrigin.Begin);
                    for (int i = 0; i < SentencesOffset.Length; i++)
                        LINBinaryWriter.Write(SentencesOffset[i]);
                }
                // END INSERTING TEXT - Only if there is text to be processed.

                // If it's a "LIN", returns to file beginning and writes the exact size of the LIN.
                if (NewFileExtension == ".lin")
                {
                    LINBinaryWriter.Seek(FileSizePos, SeekOrigin.Begin);
                    LINBinaryWriter.Write((uint)REPACKEDFILE.Length);
                }
            }
        }

        // RepackSubDirs: 1 == convert to ".pak" subdirectories too, 0 = don't repack the subdirs.
        private void RePackPAK(string PakDirToBeRepacked, string DestinationDir, int RepackSubDirs)
        {
            // Check if there are any subfolders and converts them into ".pak".
            if (RepackSubDirs == 1 && Directory.GetDirectories(PakDirToBeRepacked, "*", SearchOption.TopDirectoryOnly).Length != 0)
                foreach (string SottoCartella in Directory.GetDirectories(PakDirToBeRepacked, "*", SearchOption.TopDirectoryOnly))
                    RePackPAK(SottoCartella, PakDirToBeRepacked, 1);

            using (FileStream NEWPAK = new FileStream(Path.Combine(DestinationDir, Path.GetFileNameWithoutExtension(PakDirToBeRepacked) + ".pak"), FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter PAKBinaryWriter = new BinaryWriter(NEWPAK))
                {
                    // Stores the full address of all files included into the folder and orders them alphanumerically.
                    string[] FilesAddress = Directory.GetFiles(PakDirToBeRepacked, "*");
                    Array.Sort(FilesAddress, new AlphanumComparatorFast());

                    // Write down the n# of the sentences.
                    PAKBinaryWriter.Write((uint)FilesAddress.Length);

                    //  "SentencesOffset" will contain the offset of each file. 
                    uint[] FilesOffset = new uint[FilesAddress.Length];
                    byte Padding = 0x10; // The padding is "0x10" for every DR. 

                    // Stores the current position so that we can come back later and enter the correct offsets.
                    int pos = (int)NEWPAK.Position;

                    // Fills the pointers area with zeros. At the end of the process the area will be overwritten with the correct data.
                    for (int i = 0; i < FilesOffset.Length; i++)
                        PAKBinaryWriter.Write((uint)0x00);

                    // Padding after the pointers zone.
                    if (NEWPAK.Position % Padding != 0)
                        while (NEWPAK.Position % Padding != 0)
                            PAKBinaryWriter.Write((byte)0x0);

                    // Store the size of the area dedicated to pointers. 
                    FilesOffset[0] = (uint)NEWPAK.Position;

                    for (int i = 0; i < FilesAddress.Length; i++)
                    {
                        // It opens every single file, stores it in "BodyFile" and then inserts it into the new ".pak".
                        using (FileStream TempFile = new FileStream(FilesAddress[i], FileMode.Open, FileAccess.Read))
                        {
                            byte[] BodyFile = new byte[TempFile.Length];
                            TempFile.Read(BodyFile, 0, BodyFile.Length);
                            NEWPAK.Write(BodyFile, 0, BodyFile.Length);
                        }

                        if (i < FilesAddress.Length - 1)
                        {
                            // Inserts the padding after each file, except the last.
                            if (NEWPAK.Position % Padding != 0)
                                while (NEWPAK.Position % Padding != 0)
                                    PAKBinaryWriter.Write((byte)0x0);

                            // No need to memorize the last offset because it would point to the EOF. 
                            FilesOffset[i + 1] = (uint)NEWPAK.Position;
                        }
                    }

                    // Comes back in the area dedicated to the offsets and overwrites all the zeros with the correct offsets.
                    PAKBinaryWriter.Seek(pos, SeekOrigin.Begin);
                    for (int i = 0; i < FilesAddress.Length; i++)
                        PAKBinaryWriter.Write(FilesOffset[i]);
                }
            }
        }

        private string TextBetweenTAGsReader(string FileTranslated, string DirTextToBeRepacked)
        {
            string Sentence = null;
            int OffsetOpTAG = 0, // Stores the location where the opening TAG begins.
            OffsetEdTAG = 0; // Stores the location where the ending TAG begins.

            if (!FileTranslated.Contains("<text lang=\"ja\" />"))
            {
                string OpTag = "<text lang=\"ja\">", // It will contain the opening tag, this way it will be easier for us to know its total length.
                EdTag = "</text>"; // It will contain the ending tag, this way it will be easier for us to know its total length.

                // If the user has translated the file into their language, then change the OpTag.
                if (FileTranslated.Contains("<text lang=\"en\">"))
                    OpTag = "<text lang=\"en\">";
                else // Else delete everything that stands before the Japanese OpTag.
                    FileTranslated = FileTranslated.Substring(FileTranslated.IndexOf("<text lang=\"ja\">"));

                OffsetOpTAG = FileTranslated.IndexOf(OpTag); // Saves the opening TAG location.
                OffsetEdTAG = FileTranslated.IndexOf(EdTag); // Saves the ending TAG location.

                if (OffsetOpTAG >= 0 && OffsetEdTAG >= 0)
                {
                    int SentenceSize = (OffsetEdTAG + EdTag.Length) - OffsetOpTAG;

                    char[] temp = new char[SentenceSize];

                    /* Copy the phrase chosen within TEMP.
                    (Start point of reading, variable where the phrase will be saved, start point of writing, sentences size. */
                    FileTranslated.CopyTo(OffsetOpTAG, temp, 0, SentenceSize);

                    // TEMP2 allows us to easily remove the new lines and the tags before store them permanently.
                    string temp2 = new string(temp);

                    // Remove the extra new lines.
                    temp2 = temp2.Replace(OpTag + "\n", OpTag);
                    temp2 = temp2.Replace("\n" + EdTag, EdTag);

                    // Removes the TAGs from the sentence.
                    temp2 = temp2.Replace(OpTag, null);
                    temp2 = temp2.Replace(EdTag, null);

                    if (temp2.Length == 0 || temp2 == "\n\n" && OpTag != "<text lang=\"ja\">") // If temp2 == it's empty, then read the original sentence.
                    {
                        temp = null;
                        temp2 = null;

                        OpTag = "< text lang =\"ja\">";
                        FileTranslated = FileTranslated.Substring(FileTranslated.IndexOf("<text lang=\"ja\">"));

                        OffsetOpTAG = FileTranslated.IndexOf(OpTag); // Saves the opening TAG location.
                        OffsetEdTAG = FileTranslated.IndexOf(EdTag); // Saves the ending TAG location.

                        if (OffsetOpTAG >= 0 && OffsetEdTAG >= 0)
                        {
                            SentenceSize = (OffsetEdTAG + EdTag.Length) - OffsetOpTAG;

                            temp = new char[SentenceSize];

                            /* Copy the phrase chosen within TEMP.
                            (Start point of reading, variable where the phrase will be saved, start point of writing, sentences size. */
                            FileTranslated.CopyTo(OffsetOpTAG, temp, 0, SentenceSize);

                            // TEMP2 allows us to easily remove the new lines and the tags before store them permanently.
                            temp2 = new string(temp);

                            // Remove the extra new lines.
                            temp2 = temp2.Replace(OpTag + "\n", OpTag);
                            temp2 = temp2.Replace("\n" + EdTag, EdTag);

                            // Removes the TAGs from the sentence.
                            temp2 = temp2.Replace(OpTag, null);
                            temp2 = temp2.Replace(EdTag, null);
                        }
                    }

                    if (temp2 != null)
                        Sentence = temp2.Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">");
                    // Put a new line so the translator hasn't have to do it by himself.
                    if (DirTextToBeRepacked.Contains("novel") && temp2.Length != 0)
                        Sentence += "\n";
                }
            }
            if (Sentence == null)
                Sentence = "";

            return Sentence; // Return all the sentences.
        }

        private void button2_Click(object sender, EventArgs e) // COMPUTE PROGRESS.
        {
            if (textBox1.Text == "" || textBox1.Text == null || textBox1.Text.Contains("Click on \"Set ")) //BUILD TEXT FILES
                SetDATA01Path();

            label2.Text = "Wait..."; // Change "Ready!" to "Wait..."
            label2.Refresh(); // Refresh the Status label.

            TranslationStatus Form2 = new TranslationStatus(textBox1.Text);
            Form2.Show();

            label2.Text = "Ready!"; // Change the "Status" to "Ready!".
        }

        private void Sidekick_Load(object sender, EventArgs e)
        {

        }
    }
}
