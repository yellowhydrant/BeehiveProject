[System.Serializable]
public class Register
{
    public Submission[] submissions;

    [System.Serializable]
    public class Submission
    {
        public string blobId;
        public string submissionName;
        public string creatorName;
        public string removalPassword;
        public string creationDate;
    }
}
