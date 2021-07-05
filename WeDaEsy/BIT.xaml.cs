using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace WeDaESy
{
    /// <summary>
    /// Page with questions and Instructions needed to conduct the Brief Inventory of Thriving
    /// </summary>
    public sealed partial class BIT : Page
    {
        #region Public Properties

        ///<summary>
        ///
        ///</summary>
        public double[] BIT_Values { get; set; } = new double[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        ///<summary>
        ///
        ///</summary>
        public string VPCode { get; set; }

        #endregion Public Properties

        #region Private Fields

        private JObject BIT_ValuesJson { get; set; } = new JObject
        {
            {"BIT_1", 0},
            {"BIT_2", 0},
            {"BIT_3", 0},
            {"BIT_4", 0},
            {"BIT_5", 0},
            {"BIT_6", 0},
            {"BIT_7", 0},
            {"BIT_8", 0},
            {"BIT_9", 0},
            {"BIT_10", 0},
            {"BIT_Score",0.0 },
        };

        #endregion Private Fields

        #region Public Constructors

        ///<summary>
        ///
        ///</summary>
        public BIT()
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

        #region UI Eventhandler

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void BIT_Item_Checked(object sender, RoutedEventArgs e)
        {
            BIT_Values[Grid.GetRow(e.OriginalSource as FrameworkElement) - 1] = Grid.GetColumn(e.OriginalSource as FrameworkElement);
            BIT_ValuesJson["BIT_" + Grid.GetRow(e.OriginalSource as FrameworkElement)] = Grid.GetColumn(e.OriginalSource as FrameworkElement);
        }

        private async void Btn_BIT_finished_Click(object sender, RoutedEventArgs e)
        {
            double average = BIT_Values.Average();
            BIT_Values.Prepend(average);
            //calculate Score
            BIT_ValuesJson["BIT_Score"] = average;

            try
            {
                //completion check
                if(BIT_Values.Count(entry => entry != 0) != 10)
                {
                    throw new Exception("Bitte vervollständigen Sie den Fragebogen");
                }
                // add VP Code
                BIT_ValuesJson.First.AddBeforeSelf(new JProperty("vp_Code", VPCode));
                // log Results
                HelperQuestionPages.LogResults(results: BIT_ValuesJson, name: "BIT");
                //pass relevant information to MainPage
                BitNavArgs navParams = new BitNavArgs()
                {
                    ObtainedVpCode = VPCode,
                    BitScore = average
                };
                Frame.Navigate(typeof(MainPage), navParams);
                await Dialogs.DisplayInfoBoxAsync("BIT Antworten gespeichert.");
            }
            catch(Exception ex)
            {
                await Dialogs.DisplayInfoBoxAsync("Fehler:\n" + ex.Message);
            }
        }

        #endregion UI Eventhandler

        #endregion Private Methods
    }

    ///<summary>
    ///
    ///</summary>
    public class BitNavArgs
    {
        #region Public Properties

        ///<summary>
        ///
        ///</summary>
        public double BitScore { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string ObtainedVpCode { get; set; }

        #endregion Public Properties
    }
}