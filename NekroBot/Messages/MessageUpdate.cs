namespace NekroBot.Messages
{
    public class MessageUpdate
    {
        public string Sender { get; set; }

        public string Source { get; set; }

        public string Text { get; set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return $"Sender: {Sender}, Source: {Source}, Text: {Text}";
        }
    }
}