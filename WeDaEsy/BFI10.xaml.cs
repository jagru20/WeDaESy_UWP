using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace WeDaESy
{
    ///<summary>
    ///
    ///</summary>
    public sealed partial class BFI10 : Page
    {
        #region Private Fields

        
        ///<summary>
        ///
        ///</summary>
        public double[] BFI10_Values { get; set; } = new double[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        /// <summary>
        ///
        /// </summary>
        private int[] ReversedCode { get; set; } = new int[5] { 5, 4, 3, 2, 1 };

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        ///
        /// </summary>
        ///<summary>
        ///
        ///</summary>
        public string VPCode { get; set; }

        #endregion Public Properties

        #region Private Properties

        private JObject BFI10_Values_JSON { get; set; } = new JObject
        {
            {"Extraversion_R", 0 },
            {"Agreeableness", 0 },
            {"Conscientiousness_R", 0},
            {"Neuroticism_R",0 },
            {"Openness_R",0 },
            {"Extraversion", 0},
            {"Agreeableness_R",0 },
            {"Conscientiousness",0 },
            {"Neuroticism", 0},
            {"Openness",0 },
            {"Extraversion_Score",0.0 },
            {"Agreeableness_Score",0.0 },
            {"Conscientiousness_Score",0.0 },
            {"Neuroticism_Score",0.0 },
            {"Openness_Score", 0.0}
        };

        #endregion Private Properties

        #region Public Constructors

        ///<summary>
        ///
        ///</summary>
        public BFI10()
        {
            this.InitializeComponent();
        }

        #endregion Public Constructors

        #region Protected Methods

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(MainPage.VPCode == "")
            {//make sure there is a VPCode
                _ = Dialogs.DisplayVPCodeWarningDialogAsync(this);
            }
            else
            {
                VPCode = MainPage.VPCode;
            }
            base.OnNavigatedTo(e);
        }

        #endregion Protected Methods

        #region Private Methods

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void BFI10_Item_Checked(object sender, RoutedEventArgs e)
        {
            BFI10_Values[Grid.GetRow(e.OriginalSource as FrameworkElement) - 1] = Grid.GetColumn(e.OriginalSource as FrameworkElement);
            //find out, which item the current radiobutton belongs to
            switch(Grid.GetRow(sender as FrameworkElement))
            {
                case 1:// row 1 means extraversion, reversed item
                    BFI10_Values_JSON["Extraversion_R"] = ReversedCode[Grid.GetColumn(e.OriginalSource as FrameworkElement)-1];
                    break;

                case 2:// row 2 means Agreeableness
                    BFI10_Values_JSON["Agreeableness"] = Grid.GetColumn(e.OriginalSource as FrameworkElement);
                    break;

                case 3:// row 3 means conscientiousness, reversed item
                    BFI10_Values_JSON["Conscientiousness_R"] = ReversedCode[Grid.GetColumn(e.OriginalSource as FrameworkElement)-1];
                    break;

                case 4:// row 4 means neuroticism, reversed item
                    BFI10_Values_JSON["Neuroticism_R"] = ReversedCode[Grid.GetColumn(e.OriginalSource as FrameworkElement)-1];
                    break;

                case 5:// row 5 means openness, reversed item
                    BFI10_Values_JSON["Openness_R"] = ReversedCode[Grid.GetColumn(e.OriginalSource as FrameworkElement)-1];
                    break;

                case 6:// row 6 means Extraversion
                    BFI10_Values_JSON["Extraversion"] = Grid.GetColumn(e.OriginalSource as FrameworkElement);
                    break;

                case 7:// row 7 means agreeableness, reversed item
                    BFI10_Values_JSON["Agreeableness_R"] = ReversedCode[Grid.GetColumn(e.OriginalSource as FrameworkElement) - 1];
                    break;

                case 8:// row 8 means Conscientiousness
                    BFI10_Values_JSON["Conscientiousness"] = Grid.GetColumn(e.OriginalSource as FrameworkElement);
                    break;

                case 9:// row 9 means Neuroticism
                    BFI10_Values_JSON["Neuroticism"] = Grid.GetColumn(e.OriginalSource as FrameworkElement);
                    break;

                case 10:// row 10 means Openness
                    BFI10_Values_JSON["Openness"] = Grid.GetColumn(e.OriginalSource as FrameworkElement);
                    break;
            }
        }

        private async void Btn_BFI10_finished_Click(object sender, RoutedEventArgs e)
        {
            // calculate scores
            BFI10_Values_JSON["Extraversion_Score"] = (BFI10_Values_JSON["Extraversion_R"].Value<int>() + BFI10_Values_JSON["Extraversion"].Value<int>()) / 2.0;
            BFI10_Values_JSON["Agreeableness_Score"] = (BFI10_Values_JSON["Agreeableness_R"].Value<int>() + BFI10_Values_JSON["Agreeableness"].Value<int>()) / 2.0;
            BFI10_Values_JSON["Conscientiousness_Score"] = (BFI10_Values_JSON["Conscientiousness_R"].Value<int>() + BFI10_Values_JSON["Conscientiousness"].Value<int>()) / 2.0;
            BFI10_Values_JSON["Neuroticism_Score"] = (BFI10_Values_JSON["Neuroticism_R"].Value<int>() + BFI10_Values_JSON["Neuroticism"].Value<int>()) / 2.0;
            BFI10_Values_JSON["Openness_Score"] = (BFI10_Values_JSON["Openness_R"].Value<int>() + BFI10_Values_JSON["Openness"].Value<int>()) / 2.0;

            try
            {
                //completion check
                if(BFI10_Values.Count(entry => entry != 0) != 10)
                {
                    throw new Exception("Bitte vervollständigen Sie den Fragebogen");
                }
                BFI10_Values_JSON.First.AddBeforeSelf(new JProperty("vp_Code", VPCode));
                HelperQuestionPages.LogResults(BFI10_Values_JSON, "BFI10");
                Bfi10NavArgs navParams = new Bfi10NavArgs()
                {
                    ObtainedVpCode = VPCode,
                    ScoreConscientiousness = BFI10_Values_JSON["Conscientiousness_Score"].Value<float>(),
                    ScoreExtraversion = BFI10_Values_JSON["Extraversion_Score"].Value<float>(),
                    ScoreNeuroticism = BFI10_Values_JSON["Neuroticism_Score"].Value<float>()
                };
                Frame.Navigate(typeof(MainPage), navParams);
                await Dialogs.DisplayInfoBoxAsync("BFI10 Antworten gespeichert");
            }
            catch(System.Exception ex)
            {
                await Dialogs.DisplayInfoBoxAsync("Fehler:\n" + ex.Message);
            }
        }

        #endregion Private Methods
    }

    ///<summary>
    ///
    ///</summary>
    public class Bfi10NavArgs
    {
        #region Public Properties

        ///<summary>
        ///
        ///</summary>
        public string ObtainedVpCode { get; set; }

        ///<summary>
        ///
        ///</summary>
        public float ScoreConscientiousness { get; set; }

        ///<summary>
        ///
        ///</summary>
        public float ScoreExtraversion { get; set; }

        ///<summary>
        ///
        ///</summary>
        public float ScoreNeuroticism { get; set; }

        #endregion Public Properties
    }
}