using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static WeDaESy.GlobalSettings;

namespace WeDaESy
{
    /// <summary>
    /// Declares Group Types of the current experimental setup
    /// </summary>
    ///     ///<returns>
    ///<list type="bullet">
    /// <item><see cref="ControlGroup"/></item>
    /// <item><see cref="ExperimentalGroup1"/></item>
    /// <item><see cref="ExperimantalGroup2"/></item>
    /// </list>
    /// </returns>
    public enum GroupType
    {
        /// <summary>
        /// ControlGroup
        /// </summary>
        ControlGroup,

        /// <summary>
        /// First experimental group
        /// </summary>
        ExperimentalGroup1,

        /// <summary>
        /// Second experimental group
        /// </summary>
        ExperimantalGroup2
    }

    ///<summary>
    /// Describes how to proceed with the workload.
    ///</summary>
    ///<returns>
    ///<list type="bullet">
    /// <item><see cref="Minimize"/> = -7</item>
    /// <item><see cref="Decrease"/> = -3</item>
    /// <item><see cref="Retain"/> = 0</item>
    /// <item><see cref="Increase"/> = 3</item>
    /// <item><see cref="Maximize"/> = 7</item>
    /// </list>
    /// </returns>
    public enum NextTaskWL
    {
        /// <summary>
        /// The option to choose, if the next task should be of the lowest difficulty
        /// </summary>
        Minimize = -7,

        /// <summary>
        /// The option to choose, if the next task should be of the next easier category
        /// </summary>
        Decrease = -3,

        /// <summary>
        /// The option to choose, if the next task should be of the same difficulty
        /// </summary>
        Retain = 0,

        /// <summary>
        /// The option to choose, if the next task should be of the next more difficult category
        /// </summary>
        Increase = 3,

        /// <summary>
        /// The option to choose, if the next task should be of the highest difficulty
        /// </summary>
        Maximize = 7
    }

    ///<summary>
    /// Provides possible Names of answering scales that can be used.
    ///</summary>
    public enum ScaleNames
    {
        ///<summary>
        /// Use for a scale that is in the range of 1-100
        ///</summary>
        Scale100,

        ///<summary>
        /// Use for a 5-point Scale
        ///</summary>
        Scale5,

        ///<summary>
        /// Use for a 7-point Scale
        ///</summary>
        Scale7,

        ///<summary>
        /// Use for a 10-point Scale
        ///</summary>
        Scale10
    }

    ///<summary>
    /// Categories of task-difficulty
    ///</summary>
    public enum TaskDifficulty
    {
        ///<summary>
        /// easy task
        ///</summary>
        easy,

        ///<summary>
        /// medium task
        ///</summary>
        medium,

        ///<summary>
        /// difficult task
        ///</summary>
        difficult
    }

    ///<summary>
    /// Represents a workload Value used to compare against.
    ///</summary>
    public class WorkloadCompVal
    {
        #region Public Fields

        ///<summary>
        /// The difficulty category into which this <see cref="WorkloadCompVal"/> falls.
        ///</summary>
        [XmlElement("Difficulty")]
        public TaskDifficulty Difficulty { get; set; }

        ///<summary>
        /// The ID of the Task, this <see cref="WorkloadCompVal"/> has been obtained for.
        ///</summary>
        [XmlAttribute("TaskID", DataType = "int")]
        public int TaskID { get; set; }

        #endregion Public Fields

        #region Public Properties

        ///<summary>
        /// The actual workload score that has been obtained for this <see cref="TaskID"/>
        ///</summary>
        [XmlElement("WorkloadValue")]
        public double WorkloadValue { get; set; }

        #endregion Public Properties

        #region Public Constructors

        ///<summary>
        /// Constructor
        ///</summary>
        public WorkloadCompVal()
        {
        }

        ///<summary>
        /// Constructor that takes the TaskDifficulty, the obtained workloadValue and the task id
        ///</summary>
        public WorkloadCompVal(TaskDifficulty difficulty, double workloadValue, int id)
        {
            Difficulty = difficulty;
            WorkloadValue = workloadValue;
            TaskID = id;
        }

        #endregion Public Constructors
    }

    ///<summary>
    /// Contains a list of <see cref="WorkloadCompVal"/>s and a listname.
    ///</summary>
    [XmlRoot("WorkloadCompValList")]
    public class WorkloadCompValArray
    {
        #region Public Fields

        ///<summary>
        /// Name of this <see cref="WorkloadCompValArray"/>
        ///</summary>
        [XmlElement("Listname")]
        public string Listname { get; private set; }

        ///<summary>
        /// Array of <see cref="WorkloadCompVal"/>s
        ///</summary>
        [XmlArray("WorkloadCompValArray")]
        [XmlArrayItem("WorkloadCompVal")]
        public WorkloadCompVal[] WorkloadCompVals { get; private set; }

        #endregion Public Fields

        #region Public Constructors

        ///<summary>
        /// Initialize an empty <see cref="WorkloadCompValArray"/>
        ///</summary>
        public WorkloadCompValArray()
        {
        }

        ///<summary>
        /// Initialize a <see cref="WorkloadCompValArray"/> with a given listname and array of <see cref="WorkloadCompVal"/>s
        ///</summary>
        public WorkloadCompValArray(string listname, WorkloadCompVal[] workloadCompVals)
        {
            Listname = listname;
            try
            {
                WorkloadCompVals = workloadCompVals ?? throw new ArgumentNullException(nameof(workloadCompVals));
            }
            catch (ArgumentNullException ex)
            {
                _ = Dialogs.DisplayInfoBoxAsync(ex.Message);
            }
        }

        #endregion Public Constructors
    }

    internal class TaskFinder
    {
        #region Public Methods

        /// <summary>
        /// Determines how the workload must change for the next task, based on the satisfaction, perceived workload and personality.
        /// </summary>
        /// <param name="lastTaskId"></param>
        /// <param name="TlxScore"></param>
        /// <param name="satisfactionScore"></param>
        /// <param name="conscientiousnessScore"></param>
        /// <param name="neuroticismScore"></param>
        /// <param name="extraversionScore"></param>
        /// <returns><see cref="NextTaskWL"/> with the information if/how the difficulty of the next task should differ from the last one.</returns>
        public static NextTaskWL FindTaskWL(int lastTaskId, double TlxScore, int satisfactionScore, float conscientiousnessScore, float neuroticismScore, float extraversionScore)
        {
            // Wenn MainPage.UsePwbForCurrentTask==true dann PWB mit einbeziehen, i.e. wenn PWB hoch, dann prüfe:
            // wenn Zufriedenheit hoch, dann WL beibehalten. Wenn Zufriedenheit niedrig && Workload hoch, dann WL verringern sonst erhöhen

            if (MainPage.Current.UsePwbForNextTask == true)
            {
                // PWB berücksichtigen
                if (IsHigh(LastSurveyAnswers.Score_Bit, ScaleNames.Scale5))
                {
                    // Wenn PWB hoch, dann prüfe die Zufriedenheit
                    if (IsHigh(LastSurveyAnswers.Score_Satisfaction, ScaleNames.Scale100))
                    {
                        //Wenn Zufriedenheit hoch, dann Workload beibehalten
                        return NextTaskWL.Retain;
                    }
                    else if (IsHigh(LastSurveyAnswers.Score_Tlx, ScaleNames.Scale100))
                    {
                        //Wenn Zufriedenheit niedrig und Workload hoch, dann WL verringern
                        return NextTaskWL.Decrease;
                    }
                    else
                    {
                        //Wenn Zufriedenheit niedrig und Workload niedrig, dann WL erhöhen
                        return NextTaskWL.Increase;
                    }
                }
            }
            int evidenceSum = 0;
            //satisfaction high?
            if (IsHigh(satisfactionScore, ScaleNames.Scale100))
            {
                //is Workload higher than the associated WL Score in List?
                if (TlxScore > GlobalWorkloadCompValArray.WorkloadCompVals.SingleOrDefault(x => x.WorkloadValue != 0 && x.TaskID == lastTaskId).WorkloadValue)
                {
                    //workload higher.
                    //is neuroticism high?
                    if (IsHigh(neuroticismScore, ScaleNames.Scale5))
                    {
                        //yes: add -3 to evidence sum
                        evidenceSum -= 3;
                    }
                    else if (!IsHigh(neuroticismScore, ScaleNames.Scale5))
                    {
                        //no: do nothing (i.e. add 0 to the evidenceSum)
                        evidenceSum += 0;
                    }

                    //is conscientiousness high?
                    if (IsHigh(conscientiousnessScore, ScaleNames.Scale5))
                    {
                        //yes:
                        evidenceSum += 0;
                    }
                    else if (!IsHigh(conscientiousnessScore, ScaleNames.Scale5))
                    {
                        //no:
                        evidenceSum += 0;
                    }

                    //is extraversion high?
                    if (IsHigh(extraversionScore, ScaleNames.Scale5))
                    {
                        //yes:
                        evidenceSum += 0;
                    }
                    else if (!IsHigh(extraversionScore, ScaleNames.Scale5))
                    {
                        //no:
                        evidenceSum += 3;
                    }
                }
                else
                {
                    //workload not higher.

                    if (IsHigh(neuroticismScore, ScaleNames.Scale5))
                    {
                        //yes:
                        evidenceSum -= 0;
                    }
                    else if (!IsHigh(neuroticismScore, ScaleNames.Scale5))
                    {
                        //no:
                        evidenceSum += 3;
                    }

                    //is conscientiousness high?
                    if (IsHigh(conscientiousnessScore, ScaleNames.Scale5))
                    {
                        //yes:
                        evidenceSum += 3;
                    }
                    else if (!IsHigh(conscientiousnessScore, ScaleNames.Scale5))
                    {
                        //no:
                        evidenceSum -= 0;
                    }

                    //is extraversion high?
                    if (IsHigh(extraversionScore, ScaleNames.Scale5))
                    {
                        //yes:
                        evidenceSum += 3;
                    }
                    else if (!IsHigh(extraversionScore, ScaleNames.Scale5))
                    {
                        //no:
                        evidenceSum += 0;
                    }
                }
            }
            else
            {
                //satisfaction low
                if (TlxScore > GlobalWorkloadCompValArray.WorkloadCompVals.SingleOrDefault(x => x.WorkloadValue != 0 && x.TaskID == lastTaskId).WorkloadValue)
                {
                    //workload higher.
                    //is neuroticism high?
                    if (IsHigh(neuroticismScore, ScaleNames.Scale5))
                    {
                        //yes: add -7 to evidence sum, minimize Workload
                        evidenceSum -= 7;
                    }
                    else if (!IsHigh(neuroticismScore, ScaleNames.Scale5))
                    {
                        //no: decrease workload
                        evidenceSum -= 3;
                    }

                    //is conscientiousness high?
                    if (IsHigh(conscientiousnessScore, ScaleNames.Scale5))
                    {
                        //yes:
                        evidenceSum += 0;
                    }
                    else if (!IsHigh(conscientiousnessScore, ScaleNames.Scale5))
                    {
                        //no:
                        evidenceSum -= 3;
                    }

                    //is extraversion high?
                    if (IsHigh(extraversionScore, ScaleNames.Scale5))
                    {
                        //yes:
                        evidenceSum -= 3;
                    }
                    else if (!IsHigh(extraversionScore, ScaleNames.Scale5))
                    {
                        //no:
                        evidenceSum -= 3;
                    }
                }
                else
                {
                    //workload low.

                    if (IsHigh(neuroticismScore, ScaleNames.Scale5))
                    {
                        //yes: add -3 to evidence sum
                        evidenceSum += 3;
                    }
                    else if (!IsHigh(neuroticismScore, ScaleNames.Scale5))
                    {
                        //no: do nothing (i.e. add 0 to the evidenceSum)
                        evidenceSum += 3;
                    }

                    //is conscientiousness high?
                    if (IsHigh(conscientiousnessScore, ScaleNames.Scale5))
                    {
                        //yes:
                        evidenceSum += 7;
                    }
                    else if (!IsHigh(conscientiousnessScore, ScaleNames.Scale5))
                    {
                        //no:
                        evidenceSum += 3;
                    }

                    //is extraversion high?
                    if (IsHigh(extraversionScore, ScaleNames.Scale5))
                    {
                        //yes:
                        evidenceSum += 3;
                    }
                    else if (!IsHigh(extraversionScore, ScaleNames.Scale5))
                    {
                        //no:
                        evidenceSum += 3;
                    }
                }
            }

            double evidence = evidenceSum / 3;

            if (evidence < -3)
            {
                return NextTaskWL.Minimize;
            }
            else if (-3 <= evidence && evidence < -1)
            {
                return NextTaskWL.Decrease;
            }
            else if (-1 <= evidence && evidence <= 1)
            {
                return NextTaskWL.Retain;
            }
            else if (1 < evidence && evidence <= 3)
            {
                return NextTaskWL.Increase;
            }
            else if (3 < evidence)
            {
                return NextTaskWL.Maximize;
            }
            else
            {
                throw new Exception("Neuer Workload konnte nicht bestimmt werden");
            }
        }

        /// <summary>
        /// Determines how to change the workload of the next task based on the skill-level.
        /// </summary>
        /// <param name="lastTaskId">value between 1 and 19</param>
        /// <param name="skillLevel">value between 1 and 100</param>
        /// <returns></returns>
        public static NextTaskWL FindTaskWL(int lastTaskId, double skillLevel)
        {
            if (skillLevel > 66 && lastTaskId == 1)
            {
                return NextTaskWL.Maximize;
            }
            else if (33 < skillLevel && skillLevel <= 66 && lastTaskId == 1)
            {
                return NextTaskWL.Retain;
            }
            //TODO LastTaskID muss ID der Einstiegsaufgabe entsprechen
            else if (skillLevel <= 33 && lastTaskId == 1)
            {
                return NextTaskWL.Minimize;
            }
            else if (lastTaskId != 1)
            {
                return NextTaskWL.Retain;
            }
            else
            {
                throw new Exception("Neuer Workload konnte nicht bestimmt werden");
            }
        }

        /// <summary>
        /// Finds the ID of the next task based on the skill-level.
        /// </summary>
        /// <param name="lastTaskId"></param>
        /// <param name="skillLevel"></param>
        /// <param name="workloadCompValList"></param>
        /// <returns></returns>
        public static int GetNextTaskID(int lastTaskId, double skillLevel)
        {
            try
            {
                //zuletzt genutzte Schwierigkeit herausfinden
                TaskDifficulty currentDifficulty = GlobalWorkloadCompValArray.WorkloadCompVals[lastTaskId - 1].Difficulty;
                //
                switch (FindTaskWL(lastTaskId, skillLevel))
                {
                    case NextTaskWL.Minimize:
                        return GetRandomUnusedTaskId(TaskDifficulty.easy, lastTaskId);

                    case NextTaskWL.Maximize:
                        return GetRandomUnusedTaskId(TaskDifficulty.difficult, lastTaskId);

                    case NextTaskWL.Retain:
                        return GetRandomUnusedTaskId(currentDifficulty, lastTaskId);

                    default:
                        throw new Exception("Nicht vorgesehener Rückgabewert aus der Funktion 'FindTaskWL()'");
                }
            }
            catch (Exception ex)
            {
                _ = Dialogs.DisplayInfoBoxAsync("Fehler in GetNextTaskID(skill): " + ex.Message);
                throw new Exception("Fehler in GetNextTaskID (skill): ");
            }
        }

        /// <summary>
        /// Finds the ID of the next task, based on the satisfaction, perceived workload and personality.
        /// </summary>
        /// <param name="lastTaskId"></param>
        /// <param name="TlxScore"></param>
        /// <param name="satisfactionScore"></param>
        /// <param name="conscientiousnessScore"></param>
        /// <param name="neuroticismScore"></param>
        /// <param name="extraversionScore"></param>
        /// <returns></returns>
        public static int GetNextTaskID(int lastTaskId, double TlxScore, int satisfactionScore, float conscientiousnessScore, float neuroticismScore, float extraversionScore)
        {
            try
            {
                TaskDifficulty currentDifficulty = GlobalWorkloadCompValArray.WorkloadCompVals[lastTaskId - 1].Difficulty;
                switch (FindTaskWL(lastTaskId, TlxScore, satisfactionScore, conscientiousnessScore, neuroticismScore, extraversionScore))
                {
                    case NextTaskWL.Minimize:
                        return GetRandomUnusedTaskId(TaskDifficulty.easy, lastTaskId);

                    case NextTaskWL.Decrease:
                        switch (currentDifficulty)
                        {
                            case TaskDifficulty.easy:
                            case TaskDifficulty.medium:
                                return GetRandomUnusedTaskId(TaskDifficulty.easy, lastTaskId);

                            case TaskDifficulty.difficult:
                                return GetRandomUnusedTaskId(TaskDifficulty.medium, lastTaskId);

                            default:
                                throw new Exception("Schwierigkeit nicht erkannt.");
                        }
                    case NextTaskWL.Increase:
                        switch (currentDifficulty)
                        {
                            case TaskDifficulty.difficult:
                            case TaskDifficulty.medium:
                                return GetRandomUnusedTaskId(TaskDifficulty.difficult, lastTaskId);

                            case TaskDifficulty.easy:
                                return GetRandomUnusedTaskId(TaskDifficulty.medium, lastTaskId);

                            default:
                                throw new Exception("Schwierigkeit nicht erkannt.");
                        }
                    case NextTaskWL.Maximize:
                        return GetRandomUnusedTaskId(TaskDifficulty.difficult, lastTaskId);

                    case NextTaskWL.Retain:
                        return GetRandomUnusedTaskId(currentDifficulty, lastTaskId);

                    default:
                        throw new Exception("Nicht vorgesehener Rückgabewert aus der Funktion 'FindTaskWL()'");
                }
            }
            catch (Exception ex)
            {
                _ = Dialogs.DisplayInfoBoxAsync("Fehler in GetNextTaskID(personality): " + ex.Message);
                throw new Exception("Fehler in GetNextTaskID (personality)");
            }
        }

        private static int GetRandomUnusedTaskId(TaskDifficulty difficulty, int lastTaskId)
        {
            //randomly determine a new TaskID, taking into account already used Task IDs
            //save previous TaskId as used
            ListUsedTaskIds.Add(lastTaskId);
            //which ids belong to the required level of difficulty?
            List<int> listPossibleNewIds;
            // listTasksWhereDifficultyApplies enthält alle bisher nicht verwendeten WorkloadCompVals, deren Schwierigkeit passend ist.
            IEnumerable<WorkloadCompVal> listTasksWhereDifficultyApplies = GlobalWorkloadCompValList.Where(compVal => compVal.Difficulty == difficulty && !ListUsedTaskIds.Contains(compVal.TaskID));
            listPossibleNewIds = listTasksWhereDifficultyApplies.Select(compVal => compVal.TaskID).ToList();
            //zufällige Zahl aus dem entsprechenden ID-Bereich erzeugen
            Random random = new Random();
            int newId = 0;
            if (!listPossibleNewIds.All(item => ListUsedTaskIds.Contains(item)))
            {
                //überprüfen, ob noch nutzbare ids vorhanden, sonst killt die do-while den programmablauf
                if (listPossibleNewIds.Count > 0)
                {
                    int index = random.Next(listPossibleNewIds.Count);
                    newId = listPossibleNewIds[index];
                }
                return newId;
            }
            else
            {
                throw new Exception("Keine Aufgaben in dieser Schwierigkeit mehr vorhanden!");
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Tests if the given score of scale is high or low.
        /// This means: For satisfaction and the personality dimensions returns <see langword="true"/> compare is higher than middle of scale.
        ///             For Workload, returns <see langword="true"/> if compare is higher than value in GlobalSettings.WorkloadCompVals.
        /// </summary>
        /// <param name="compare">the value to be tested</param>
        /// <param name="scale">the Scale used when obtaining "compare"</param>
        /// <returns></returns>
        public static bool IsHigh(double compare, ScaleNames scale)
        {
            switch (scale)
            {
                case ScaleNames.Scale5:
                    if (compare <= 3)
                    {
                        return false;
                    }
                    else if (compare > 3)
                    {
                        return true;
                    }
                    break;

                case ScaleNames.Scale100:
                    if (compare <= 50)
                    {
                        return false;
                    }
                    else if (compare > 50)
                    {
                        return true;
                    }
                    break;

                case ScaleNames.Scale10:
                    if (compare <= 5) { return false; } else if (compare > 5) { return true; }
                    break;

                case ScaleNames.Scale7:
                    if (compare <= 4) { return false; } else if (compare > 4) { return true; }
                    break;
            }
            throw new Exception(@"No proper ScaleName provided in 'IsHigh5()'");
        }

        #endregion Private Methods
    }
}