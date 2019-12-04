namespace FileConverter
{
    internal class Pair<T1, T2>
    {
        private T1 First { get; set; }
        private T2 Secound { get; set; }

        public Pair()
        {
        }

        public Pair(T1 first, T2 secound)
        {
            First = first; Secound = secound;
        }
    }
}