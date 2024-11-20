using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DIALOGUE
{
    public class DL_DialogueData
    {
        public List<Dialogue_Segment> segments;
        private const string segmentIdentifierPattern = @"\{[ca]\}|\{w[ca]\s\d*\.?\d*\}";

        public DL_DialogueData(string rawDialog)
        {
            segments = RipSegments(rawDialog);
        }

        private List<Dialogue_Segment> RipSegments(string rawDialog)
        {
            List<Dialogue_Segment> segments = new List<Dialogue_Segment>();
            MatchCollection matches = Regex.Matches(rawDialog, segmentIdentifierPattern);

            int lastIndex = 0;
            Dialogue_Segment segment = new Dialogue_Segment();
            segment.dialogue = matches.Count == 0 ? rawDialog : rawDialog.Substring(0, matches[0].Index);
            segment.startSingal = Dialogue_Segment.StartSingal.NONE;
            segment.singalDelay = 0;
            segments.Add(segment);

            if (matches.Count == 0)
            {
                return segments;
            }
            else
                lastIndex = matches[0].Index;


            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                segment = new Dialogue_Segment();

                //Get the start singal form segment
                string singelMatch = match.Value;
                singelMatch = singelMatch.Substring(1, match.Length - 2);
                string[] singalSplit = singelMatch.Split(' ');

                segment.startSingal = (Dialogue_Segment.StartSingal)Enum.Parse(typeof(Dialogue_Segment.StartSingal), singalSplit[0].ToUpper());

                //Delay Singal
                if (singalSplit.Length > 1)
                    float.TryParse(singalSplit[1], out segment.singalDelay);

                //Get the dialog for segment
                int nextIndex = i + 1 < matches.Count ? matches[i + 1].Index : rawDialog.Length;
                segment.dialogue = rawDialog.Substring(lastIndex + match.Length, nextIndex - (lastIndex + match.Length));

                segments.Add(segment);
            }
            return segments;
        }

        public struct Dialogue_Segment
        {
            public string dialogue;
            public StartSingal startSingal;
            public float singalDelay;
            public enum StartSingal { NONE, C, A, WA, WC }

            public bool appendTxt => startSingal == StartSingal.A || startSingal == StartSingal.WA;
        }
    }
}