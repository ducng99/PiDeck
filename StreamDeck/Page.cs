namespace StreamDeck
{
    public class Page
    {
        public static int curID = 0;
        public int id;
        public int preID;
        public Decks[] decks;
        public string name;
        public int x;
        public int y;

        public Page(bool newPage = false)
        {
            if (newPage)
            {
                name = StreamDeckPC.appname;
                preID = -1;
                x = 5;
                y = 3;
                id = curID++;
            }
        }

        public static void CountAllPages()
        {
            string[] decks = System.IO.Directory.GetFiles(FileHandler.decksFileName);

            curID = decks.Length;
        }
    }
}
