using System.Collections.Generic;

public class TrieNode
{
    public Dictionary<char, TrieNode> Children { get; set; }
    public bool IsEndOfWord { get; set; }

    public TrieNode()
    {
        Children = new Dictionary<char, TrieNode>();
        IsEndOfWord = false;
    }
}

public class Trie
{
    private TrieNode root;

    public Trie()
    {
        root = new TrieNode();
    }

    // Insert a word into the Trie
    public void Insert(string word)
    {
        TrieNode currentNode = root;
        foreach (char c in word)
        {
            if (!currentNode.Children.ContainsKey(c))
            {
                currentNode.Children[c] = new TrieNode();
            }
            currentNode = currentNode.Children[c];
        }
        currentNode.IsEndOfWord = true;
    }

    // Search for a word in the Trie
    public bool Search(string word)
    {
        TrieNode currentNode = root;
        foreach (char c in word)
        {
            if (!currentNode.Children.ContainsKey(c))
            {
                return false;
            }
            currentNode = currentNode.Children[c];
        }
        return currentNode.IsEndOfWord;
    }
}
