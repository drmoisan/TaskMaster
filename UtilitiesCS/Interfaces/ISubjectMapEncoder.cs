using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS
{
    /// <summary>
    /// Simple encoder that converts between word tokens and integers. 
    /// Integer values assigned on a FIFO basis
    /// </summary>
    public interface ISubjectMapEncoder
    {
        /// <summary>
        /// Serializable concurrent observable dictionary containing word tokens and integer values
        /// </summary>
        IScoDictionary<string, int> Encoder { get; }
        
        /// <summary>
        /// Matches array of tokens against existing values. Integers are 
        /// assigned to each new token on a FIFO basis in increments of 1
        /// </summary>
        /// <param name="tokens">String array of tokens</param>
        void AugmentTokenDict(string[] tokens);

        /// <summary>
        /// Tokenizes a string of text and matches resulting array of tokens 
        /// with existing dictionary values. Integers are assigned to each new 
        /// token on a FIFO basis in increments of 1
        /// </summary>
        /// <param name="text">string of text to be tokenized and encoded</param>
        void AugmentTokenDict(string text);
        
        /// <summary>
        /// Decodes an array of integers to an array of textual tokens
        /// </summary>
        /// <param name="encodedWords">Array of integers representing textual tokens</param>
        /// <returns>Array of string tokens</returns>
        string Decode(int[] encodedWords);

        /// <summary>
        /// Encodes an array of textual tokens as an array of integer equivalents
        /// </summary>
        /// <param name="words">Text to be tokenized and encoded</param>
        /// <returns>Array of integers</returns>
        int[] Encode(string text);

        /// <summary>
        /// Encodes an array of textual tokens as an array of integer equivalents
        /// </summary>
        /// <param name="words">Array of textual tokens</param>
        /// <returns>Array of integers</returns>
        int[] Encode(string[] words);

        /// <summary>
        /// Rebuilds encoding dictionary based and re-encodes elements within 
        /// the map passed as a variable
        /// </summary>
        /// <param name="map">Serializable list of <see cref="ISubjectMapEntry"/></param>
        void RebuildEncoding(ISubjectMapSL map);

        /// <summary>
        /// Rebuilds encoding dictionary based and re-encodes elements within 
        /// the map stored in the class
        /// </summary>
        void RebuildEncoding();
    }
}