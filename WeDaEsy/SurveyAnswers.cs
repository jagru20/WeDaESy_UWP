namespace WeDaESy
{
    ///<summary>
    ///This class stores the latest collected answers to the questionnaires conducted.
    ///</summary>
    public static class LastSurveyAnswers
    {
        #region Properties

        ///<summary>
        ///An object containing all answers to the BFI-10 questions.
        ///</summary>
        public static BFI_Results LastBfiResults { get; set; }

        ///<summary>
        ///An object containing all answers to the BIT questions.
        ///</summary>
        public static BIT_Results LastBitResults { get; set; }

        ///<summary>
        ///Array containing the Answers to the six NASA-TLX questionnaires in the respective order.
        ///</summary>
        public static double[] LastTlxAnswers { get; set; }

        ///<summary>
        ///The value for psychological well being, accessed through the Brief Inventory of Thriving (BIT).
        ///</summary>
        public static double Score_Bit { get; set; }

        ///<summary>
        ///The value obtained from the test subject for conscientiousness.
        ///</summary>
        public static float Score_Conscientiousness { get; set; }

        ///<summary>
        ///The value obtained from the test subject for extraversion.
        ///</summary>
        public static float Score_Extraversion { get; set; }

        ///<summary>
        ///The value obtained from the test subject for neuroticism.
        ///</summary>
        public static float Score_Neuroticism { get; set; }

        ///<summary>
        ///The satisfaction score.
        ///</summary>
        public static int Score_Satisfaction { get; set; }

        ///<summary>
        ///The overall NASA-TLX score.
        ///</summary>
        public static double Score_Tlx { get; set; }

        public static double Skill { get; internal set; }

        #endregion Properties
    }

    ///<summary>
    ///Class for storing the answers to the BFI-10 personality questionnaire.
    ///</summary>
    public class BFI_Results : SurveyResultsObject
    {
        #region Properties

        ///<summary>
        ///
        ///</summary>
        public string Agreeableness { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string Agreeableness_R { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string Agreeableness_Score { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string Conscientiousness { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string Conscientiousness_R { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string Conscientiousness_Score { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string Extraversion { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string Extraversion_R { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string Extraversion_Score { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string Neuroticism { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string Neuroticism_R { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string Neuroticism_Score { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string Openness { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string Openness_R { get; set; }

        ///<summary>
        ///
        ///</summary>
        public string Openness_Score { get; set; }

        #endregion Properties
    }

    ///<summary>
    ///Class for storing the answers to the BIT psychological well being questionnaire.
    ///</summary>
    public class BIT_Results : SurveyResultsObject
    {
        #region Properties

        ///<summary>
        ///
        ///</summary>
        public int BIT_1 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public int BIT_10 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public int BIT_2 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public int BIT_3 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public int BIT_4 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public int BIT_5 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public int BIT_6 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public int BIT_7 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public int BIT_8 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public int BIT_9 { get; set; }

        ///<summary>
        ///
        ///</summary>
        public int BIT_Score { get; set; }

        #endregion Properties
    }

    /// <summary>
    ///
    /// </summary>
    public abstract class SurveyResultsObject
    {
    }
}