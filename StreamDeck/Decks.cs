namespace StreamDeck
{
    public class Decks
    {
        public string Name;
        public string Action;
        public int innerPageID;
        public string image;

        public Decks()
        {
            Name = "";
            Action = "[EMPTY]";
            innerPageID = -1;
            image = "";
        }
    }
}
