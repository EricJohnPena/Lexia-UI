using System.Collections.Generic;

[System.Serializable]
public class CrosswordLevel
{
    public List<WordPlacement> fixedLayout;
    public List<WordClue> wordClues;  // Change to List
}

[System.Serializable]
public class WordPlacement
{
    public string word;
    public int startRow;
    public int startCol;
    public bool horizontal;
}

[System.Serializable]
public class WordClue
{
    public string word;
    public string clue;
}
