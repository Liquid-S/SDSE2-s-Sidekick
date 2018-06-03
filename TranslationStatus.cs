using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SDSE2_s_Sidekick
{
    public partial class TranslationStatus : Form
    {
        public TranslationStatus(string data01path)
        {
            InitializeComponent();
            ComputeProgress(data01path);
        }

        private void ComputeProgress(string data01path)
        {
            int InterventionsTranslated = 0, TotalD = 0,
            PrologueIT = 0, Chap1IT = 0, Chap2IT = 0, Chap3IT = 0, Chap4IT = 0, Chap5IT = 0, Chap6IT = 0, // IT stands for "Interventions Translated".
            EpilogueIT = 0, FreeTimeIT = 0, IslandModeIT = 0, NovelIT = 0, OtherIT = 0,
            PrologueTOT = 0, Chap1TOT = 0, Chap2TOT = 0, Chap3TOT = 0, Chap4TOT = 0, Chap5TOT = 0, Chap6TOT = 0,
            EpilogueTOT = 0, FreeTimeTOT = 0, IslandModeTOT = 0, NovelTOT = 0, OtherTOT = 0;

            List<string> FilesYetToBeTranslated = new List<string>();

            foreach (string TXTFile in Directory.GetFiles(data01path, "*.txt", SearchOption.AllDirectories))
            {
                // The txt's content is saved inside "lineENG".
                using (FileStream TXT = new FileStream(TXTFile, FileMode.Open, FileAccess.Read))
                using (StreamReader txtEditor = new StreamReader(TXT, Encoding.UTF8))
                {
                    string lineENG = txtEditor.ReadToEnd(), DirName = TXTFile;

                    /* Saves the path and delete whatever comes before "data01".
                     This way it will be easier to find out from what chapter the txt is. */
                    DirName = DirName.Replace(data01path, null);

                    /* Let's count how many files have been translated until now.
                      If the TXT contains "<text lang="en">" that means that it has been translated.
                      The other two conditions "||" are needed because we need to count empty interventions too. */
                    if (lineENG.Contains("<text lang=\"en\">") || lineENG.Contains("<text lang=\"ja\" />") || lineENG.Contains("<text lang=\"ja\"></text>"))
                    {
                        InterventionsTranslated++;
                        if (DirName.Contains("e00"))
                            PrologueIT++;
                        else if (DirName.Contains("e01"))
                            Chap1IT++;
                        else if (DirName.Contains("e02"))
                            Chap2IT++;
                        else if (DirName.Contains("e03"))
                            Chap3IT++;
                        else if (DirName.Contains("e04"))
                            Chap4IT++;
                        else if (DirName.Contains("e05"))
                            Chap5IT++;
                        else if (DirName.Contains("e06"))
                            Chap6IT++;
                        else if (DirName.Contains("e07"))
                            EpilogueIT++;
                        else if (DirName.Contains("e08"))
                            FreeTimeIT++;
                        else if (DirName.Contains("e09"))
                            IslandModeIT++;
                        else if (DirName.Contains("novel"))
                            NovelIT++;
                        else
                            OtherIT++;
                    }
                    else // If the TXT hasn't been translated, then add it to the list "FilesYetToBeTranslated". 
                        FilesYetToBeTranslated.Add(DirName);

                    // Let's count the total amount of interventions the game has.
                    if (DirName.Contains("e00"))
                        PrologueTOT++;
                    else if (DirName.Contains("e01"))
                        Chap1TOT++;
                    else if (DirName.Contains("e02"))
                        Chap2TOT++;
                    else if (DirName.Contains("e03"))
                        Chap3TOT++;
                    else if (DirName.Contains("e04"))
                        Chap4TOT++;
                    else if (DirName.Contains("e05"))
                        Chap5TOT++;
                    else if (DirName.Contains("e06"))
                        Chap6TOT++;
                    else if (DirName.Contains("e07"))
                        EpilogueTOT++;
                    else if (DirName.Contains("e08"))
                        FreeTimeTOT++;
                    else if (DirName.Contains("e09"))
                        IslandModeTOT++;
                    else if (DirName.Contains("novel"))
                        NovelTOT++;
                    else
                        OtherTOT++;
                }
            }

            // Print the list of interventions that hasn't been translated yet.
            if (FilesYetToBeTranslated.Count > 0)
            {
                using (FileStream TXTFIle = new FileStream("Files_yet_to_be_translated.txt", FileMode.Create, FileAccess.Write))
                using (BinaryWriter TXTBin = new BinaryWriter(TXTFIle))
                    for (int i = 0; i < FilesYetToBeTranslated.Count; i++)
                        TXTBin.Write((FilesYetToBeTranslated[i] + "\n").ToCharArray());
            }

            // TottalD = Total amount of interventions in the game. 
            TotalD = PrologueTOT + Chap1TOT + Chap2TOT + Chap3TOT + Chap4TOT + Chap5TOT + Chap6TOT + EpilogueTOT + FreeTimeTOT + IslandModeTOT + NovelTOT + OtherTOT;

            // Calculates the amount of interventions translated.
            double PercentageTOT = ((double)InterventionsTranslated / (double)TotalD) * 100;
            double PercentagePrologue = ((double)PrologueIT / (double)PrologueTOT) * 100;
            double PercentageChap1 = ((double)Chap1IT / (double)Chap1TOT) * 100;
            double PercentageChap2 = ((double)Chap2IT / (double)Chap2TOT) * 100;
            double PercentageChap3 = ((double)Chap3IT / (double)Chap3TOT) * 100;
            double PercentageChap4 = ((double)Chap4IT / (double)Chap4TOT) * 100;
            double PercentageChap5 = ((double)Chap5IT / (double)Chap5TOT) * 100;
            double PercentageChap6 = ((double)Chap6IT / (double)Chap6TOT) * 100;
            double PercentageEpilogue = ((double)EpilogueIT / (double)EpilogueTOT) * 100;
            double PercentageFreeTime = ((double)FreeTimeIT / (double)FreeTimeTOT) * 100;
            double PercentageIslandMode = ((double)IslandModeIT / (double)IslandModeTOT) * 100;
            double PercentageNovel = ((double)NovelIT / (double)NovelTOT) * 100;
            double PercentageOther = ((double)OtherIT / (double)OtherTOT) * 100;

            // Update the progressbar's value.
            progressBar1.Value = (int)PercentageTOT;

            // The data is saved inside the labels.
            labelPrologueIT.Text = PrologueIT + " / " + PrologueTOT + "  (" + PercentagePrologue.ToString("#.##") + "%)";
            labelChap1IT.Text = Chap1IT + " / " + Chap1TOT + "  (" + PercentageChap1.ToString("#.##") + "%)";
            labelChap2IT.Text = Chap2IT + " / " + Chap2TOT + "  (" + PercentageChap2.ToString("#.##") + "%)";
            labelChap3IT.Text = Chap3IT + " / " + Chap3TOT + "  (" + PercentageChap3.ToString("#.##") + "%)";
            labelChap4IT.Text = Chap4IT + " / " + Chap4TOT + "  (" + PercentageChap4.ToString("#.##") + "%)";
            labelChap5IT.Text = Chap5IT + " / " + Chap5TOT + "  (" + PercentageChap5.ToString("#.##") + "%)";
            labelChap6IT.Text = Chap6IT + " / " + Chap6TOT + "  (" + PercentageChap6.ToString("#.##") + "%)";
            labelEpilogueIT.Text = EpilogueIT + " / " + EpilogueTOT + "  (" + PercentageEpilogue.ToString("#.##") + "%)";
            labelFreeTimeIT.Text = FreeTimeIT + " / " + FreeTimeTOT + "  (" + PercentageFreeTime.ToString("#.##") + "%)";
            labelIslandModeIT.Text = IslandModeIT + " / " + IslandModeTOT + "  (" + PercentageIslandMode.ToString("#.##") + "%)";
            labelDRIFIT.Text = NovelIT + " / " + NovelTOT + "  (" + PercentageNovel.ToString("#.##") + "%)";
            labelOtherIT.Text = OtherIT + " / " + OtherTOT + "  (" + PercentageOther.ToString("#.##") + "%)";
            labelTotalIT.Text = InterventionsTranslated + " / " + TotalD + "  (" + PercentageTOT.ToString("#.##") + "%)";
        }
    }
}