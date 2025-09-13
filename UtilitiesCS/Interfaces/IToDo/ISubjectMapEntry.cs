using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UtilitiesCS
{
    /// <summary>
    /// Subject Map Entry holds information regarding email folders and the subject line of 
    /// the emails within the folder. Each entry contains a unique combination of a folder 
    /// name and an email subject. Class is to be used in conjunction with 
    /// </summary>
    public interface ISubjectMapEntry: IEquatable<ISubjectMapEntry>
    {
        /// <summary>
        /// List of common words to strip from tokens to make token 
        /// list as distinct as possible
        /// </summary>
        IList<string> CommonWords { get; set; }
        
        /// <summary>
        /// String with the path of an email folder relative to the inbox
        /// </summary>
        string Folderpath { get; set; }

        /// <summary>
        /// String with the name of an email folder 
        /// </summary>
        string Foldername { get; }

        /// <summary>
        /// Standardized subject field of an email or group of emails in a folder
        /// </summary>
        string EmailSubject { get; set; }

        /// <summary>
        /// Count of emails in a folder with the same standardized subject field
        /// </summary>
        int EmailSubjectCount { get; set; }

        /// <summary>
        /// Array of integers representing the folder name
        /// </summary>
        int[] FolderEncoded { get; set; }

        /// <summary>
        /// Array of integers representing the lengths of each word in the folder name
        /// </summary>
        int[] FolderWordLengths { get; set; }

        void LogObjectState();

        /// <summary>
        /// Array of integers representing the subject
        /// </summary>
        int[] SubjectEncoded { get; set; }

        /// <summary>
        /// Array of integers representing the lengths of each word in the subject
        /// </summary>
        int[] SubjectWordLengths { get; set ; }

        /// <summary>
        /// Smith Waterman score
        /// </summary>
        int Score { get; set; }

        /// <summary>
        /// Reference to encoder to be used to encode the folder name and subject
        /// </summary>
        public ISubjectMapEncoder Encoder { get;  set; }

        /// <summary>
        /// Tokenize Foldername and Subject and Encode using the supplied 
        /// tokenizer regex pattern and encoder
        /// </summary>
        /// <param name="encoder">Maps the tokenized strings to integer equivalents</param>
        /// <param name="tokenizerRegex">Regex pattern to tokenize text</param>
        void Encode(ISubjectMapEncoder encoder, Regex tokenizerRegex);

        /// <summary>
        /// Encode Foldername and Subject using the supplied encoder  
        /// </summary>
        /// <param name="encoder">
        /// Maps the tokenized strings to integer equivalents
        /// </param>
        public void Encode(ISubjectMapEncoder encoder);

        /// <summary>
        /// Encode the array of tokens using the supplied encoder 
        /// </summary>
        /// <param name="encoder">Maps the tokenized strings to integer equivalents</param>
        /// <param name="tokens">Array of tokens to be encoded</param>
        /// <returns></returns>
        int[] Encode(ISubjectMapEncoder encoder, string[] tokens);

        /// <summary>
        /// Encode Foldername and Subject. Encoder must have already been initialized
        /// </summary>
        //public void Encode();

        /// <summary>
        /// Determine if the class members are ready to be encoded. 
        /// </summary>
        /// <param name="encoder"><inheritdoc cref="ISubjectMapEncoder"/></param>
        /// <returns></returns>
        bool ReadyToEncode(ISubjectMapEncoder encoder);

        /// <summary>
        /// Determine if the class members are ready to be encoded.
        /// </summary>
        /// <param name="throwEx">Flag to determine if returns false or throws exception</param>
        /// <returns>true or false</returns>
        bool ReadyToEncode(bool throwEx);

        /// <summary>
        /// Determine if the array of tokens is ready to be encoded
        /// </summary>
        /// <param name="tokens">String array of tokens</param>
        /// <param name="throwEx">Flag to determine if returns false or throws exception</param>
        /// <returns>true or false</returns>
        bool ReadyToEncode(string[] tokens, bool throwEx);

        bool TryRepair(bool encode);

        bool Validate();
    }
}