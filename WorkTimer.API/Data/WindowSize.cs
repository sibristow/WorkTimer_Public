namespace WorkTimer4.API.Data
{
    public record struct WindowSize
    {
        public double Width { get;set; }

        public double Height { get;set; }

        public WindowSize()
            : this(800,450)
        {
        }

        public WindowSize(double width, double height)
        {
            this.Width = width;
            this.Height = height;
        }
    }
}
