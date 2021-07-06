# WeDaESy
_**We**_ rable _**Da**_ ta _**E**_ xchange _**Sy**_ stem is the combination of two Apps. The first, a Universal Windows Platform (UWP) App, is able to receive and process Data sent from a Samsung Gear S3 device. The two Apps have been used for a scientific experiment, investigating reationships between personalty, well being, task performance, task characteristics, mental effort and satisfaction. The experimental procedure and the role of the apps in it will be described below briefly. For the communication between PC and Smartwatch, a mqtt broker needs to be set up that can be reached by both devices.

# Experiment
First off, participants were asked to answer demographic questionnaires, a questionnaire on psychological well being as well as a questionnaire on personality. For this purpose, special user interfaces were programmed into the UWP App (BIT.xaml, BFI10.xaml), which were used by the test subjects and enabled a subsequent real-time evaluation of the answers given. Thus, after answering the questionnaires, information was available on the extent to which the individual personality traits were present in each test subject.
During the experiment, the participants had to complete variant tasks. After each task, the NASA-TLX and satisfaction questionnaires were launched by the experimenter through the UWP app. The participints had then the opportunity to answer the questions on the Smartwatch on their wrist. Once the questionnaires were processed, the app behaved differently depending on which group the current participant was in. If the participant was in the control condition, the app randomly selected a task that corresponded to the ability level indicated in the demographic questionnaire and had not yet been completed by this person. The following thresholds were chosen for this purpose: Skill 1-33 = easy tasks, skill 34-66 = intermediate tasks, skill 67-100 = difficult tasks. As long as the subject was in the experimental condition, the data received from the smartwatch was processed in real time and used to suggest a new task for the subject. This was done depending on the individual's personality traits, PWB, and reported satisfaction and workload values. The algorithm used for task assignment is described below from concept to implementation and illustrated with an example. The example test subject is a fictitious person who is to be assigned their first task after the initial task.
# Algorithm
For the assignment of the new task, the collected metrics (workload, satisfaction, personality traits, PWB) were evaluated based on previously defined thresholds. First, it was assessed whether the satisfaction of the participant was high or low. This was judged to be high if the last response value given to the satisfaction question was above 50. For example, if a subject indicated a satisfaction score of 68 after the initial task, then the score was rated as high. Next, the most recent workload score recorded by the NASA-TLX was compared to the average workload score reported in the pretest for that task. This was used to assess whether the workload score reported by the current participant was considered high or low. Suppose the named subject had reported a workload score of 45 after completing the entry task. Since the entry task was given a mean score of 36.16 in the pretest, the workload reported by the sample subject was classified as high. After assessing workload and satisfaction, each of the three personality traits considered was tested to determine whether it was high or low in expression. A personality trait was considered high if the value for that trait reported in the BFI-10 was greater than three. Based on findings from previous research literature, it was determined for each personality trait whether its expression in combination with the satisfaction and workload scores argued for increasing, decreasing, or maintaining task difficulty. These assumptions are listed in Table 2 below. The interaction of empirical findings transferred to the algorithm is exemplified below using an example subject. As can be seen in the Table 1, each tendency of increase, decrease, maintain is converted into numbers.

_**Table 1: Weights used for determining the change in difficulty**_
| **Tendency** | **Weight** |
|---|---|
| Minimize | \-7 |
| Decrease | \-3 |
| Retain | 0 |
| Increase | 3 |
| Maximize | 7 |

For an example person who scored high in Neuroticism and Conscientiousness and low in Extraversion, the algorithm would have collected the following clues. The high expression of Neuroticism indicated that the next assigned task should be a lower workload task. This task assignment cue was understood as a tendency to choose a lower workload task and was coded -3. The high expression of conscientiousness indicated that the workload of the next task should be equal to the workload of the previous task. The tendency to maintain task difficulty was coded as 0. Since extraversion was indicated with a lower value than 3, the workload in the following task could be chosen higher than the person had indicated it for the edited task. This increase was coded as +3. These tendencies were used to assign the next task to the subject using an algorithm. 
Thus, the operationalization of task assignment can be understood as the collection of tendencies. Depending on whether a feature expression now argued for minimizing, decreasing, maintaining, increasing, or maximizing the difficulty for the subsequent task, the tendencies were coded. In two cases, the combination argued for minimizing or maximizing task difficulty. If a person had indicated Workload Score, as well as Satisfaction low, but Conscientiousness high, this corresponded to a tendency to assign the next task with maximum Workload Score. When satisfaction and workload were low and neuroticism was high, the tendency to assign the next task with minimum difficulty was determined with the help of the literature.


_**Figure 1: Flowchart of the entire algorithm**_
![Flowchart](https://github.com/jagru20/WeDaESy_UWP/blob/main/img/flowchart.png)

_**Table 2: Theoretical findings for the decision algorithm**_
| **Expression of satisfaction and workload** | **Personality trait** | **Implication for task difficulty** | **Reason for the implications** |
|---|---|---|---|
| Low satisfaction, low perceived workload | Neuroticism | Increase | 1\) |
|  | Conscientiousness | Low pronounced: Increase | 1\) |
|  |  | Highly pronounced: Maximize | Conscientiousness correlates with performance & is a good predictor of performance, so if conscientiousness is high, high performance \(=heavy task\) should be selected \(Barrick et al\., 2001\). |
|  | Extraversion | Low pronounced: Increase | 1\) |
|  |  | Highly pronounced: Increase | Better performance is achieved when task is more demanding \(Rose et al\., 2002\)\. |
| Low Satisfaction, High Workload | Neuroticism | Low pronounced: Decrease | 2\) |
|  |  | Highly pronounced: Minimize | High neuroticism is associated with lower frustration tolerance and thus higher workload \(Rose et al., 2002\)\. |
|  | Conscientiousness | Low pronounced: Decrease | 2\) |
|  |  | Highly pronounced: Retain| Conscientiousness correlates positively with performance \(Judge & Erez, 2007\), therefore, conscientiousness may contribute to performance improvement. |
|  | Extraversion | Low pronounced: Decrease| 2\) |
|  |  | Highly pronounced: Decrease| 2\) |
| High Satisfaction, low Workload | Neuroticism | Low pronounced: Increase| 1\) |
|  |  | Highly pronounced: Retain| Neuroticism can lead to lower satisfaction and poor performance when workload is too high \(Judge et al\., 2002; Rose et al\., 2002\), hence the high satisfaction protect\. |
|  | Conscientiousness | Low pronounced: Retain| 3\) \+ Increase could lead to decrease in performance \(Judge & Erez, 2007\)\. |
|  |  | Highly pronounced: Increase| Higher performance with high conscientiousness \(Judge & Erez, 2007\)\. |
|  | Extraversion | Low pronounced: Retain| 3\) |
|  |  | Highly pronounced: Increase| To increase mental arousal with accompanying performance enhancement \(Rose et al\., 2002\)\. |
| High Satisfaction, High Workload | Neuroticism | Low pronounced: Retain| 3\) |
|  |  | Highly pronounced: Decrease| Avoid frustration positively correlated with neuroticism \(Rose et al\., 2002\). |
|  | Conscientiousness | Low pronounced: Retain| 3\) |
|  |  | Highly pronounced: Retain| 3\) |
|  | Extraversion | Low pronounced: Increase| Possibly faster mental arousal with lower difficulty \(cf. \(Rose et al\., 2002\)\. Therefore, possibly higher workload even on easy tasks, but could perhaps show better performance by finishing more difficult tasks \. High satisfaction is associated with high performance \(Edwards et al\., 2008; Halkos & Bousinakis, 2010\). |
|  |  | Highly pronounced: Retain| 3\) |

_Notes:_

1\) _No evidence in literature that given combination of workload and satisfaction should not be increased based on personality trait expression._

2\) _High workload is associated with low satisfaction, regardless of the expression of the personality trait. Therefore, workload should be decreased (Moray, 1988). At the same time, high workload leads to poorer performance, so reduce workload to better perform, even if it is easier (Wickens & Tsang, 2015). Higher workload leads to faster fatigue, which in turn has a negative impact on performance (Fan & Smith, 2017)._

3\) _High satisfaction is associated with high performance (Edwards et al., 2008; Halkos & Bousinakis, 2010). Therefore, should be maintained in this combination of difficulty._ 

# References
Barrick, M. R., Mount, M. K., & Judge, T. A. (2001). Personality and Performance at the Beginning of the New Millennium: What Do We Know and Where Do We Go Next? International Journal of Selection and Assessment, 9(1&2), 9–30. https://doi.org/10.1111/1468-2389.00160

Edwards, B. D., Bell, S. T., Arthur, J. W., & Decuir, A. D. (2008). Relationships between Facets of Job Satisfaction and Task and Contextual Performance. Applied Psychology, 57(3), 441–465. https://doi.org/10.1111/j.1464-0597.2008.00328.x

Fan, J., & Smith, A. P. (2017). The Impact of Workload and Fatigue on Performance. In L. Longo & M. C. Leva (Eds.), Communications in Computer and Information Science: Vol. 726. Human mental workload: Models and applications : First International Symposium, H-WORKLOAD 2017, Dublin, Ireland, June 28-30, 2017 : revised selected papers (Vol. 726, pp. 90–105). Springer. https://doi.org/10.1007/978-3-319-61061-0_6

Halkos, G. E., & Bousinakis, D. (2010). The effect of stress and satisfaction on productivity. The International Journal of Productivity and Performance Management : IJPPM, 59(5), 415–431.

Judge, T. A., & Erez, A. (2007). Interaction and intersection: The constellation of emotional stability and extraversion in predicting performance. Personnel Psychology, 60(3), 573–596. https://doi.org/10.1111/j.1744-6570.2007.00084.x

Judge, T. A., Heller, D., & Mount, M. K. (2002). Five-factor model of personality and job satisfaction: A meta-analysis. The Journal of Applied Psychology, 87(3), 530–541. https://doi.org/10.1037/0021-9010.87.3.530

Moray, N. (1988). Mental workload since 1979. International Review of Ergonomics, 2, 123–150.

Rose, C. L., Murphy, L. B., Byard, L., & Nikzad, K. (2002). The role of the Big Five personality factors in vigilance performance and workload. European Journal of Personality, 16(3), 185–200. https://doi.org/10.1002/per.451

Wickens, C. D., & Tsang, P. S. (2015). Workload. In D. A. Boehm-Davis, F. T. Durso, & J. D. Lee (Eds.), APA handbook of human systems integration (pp. 277–292). American Psychological Association. https://doi.org/10.1037/14528-018
