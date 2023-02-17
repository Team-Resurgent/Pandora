namespace Pandora.Network
{
    public static class DownloadDetailStore
    {
        private static List<DownloadDetail> m_downloadDetails = new();

        public static DownloadDetail[] GetReadyDownloadDetails()
        {
            return m_downloadDetails.Where(d => d.Progress == string.Empty).ToArray();
        }

        public static DownloadDetail[] GetDownloadDetails()
        {
            return m_downloadDetails.ToArray();
        }

        public static void AddDownloadDetail(DownloadDetail downloadDetail)
        {
            m_downloadDetails.Add(downloadDetail);
        }

        public static void ClearDownloadDeatails()
        {
            m_downloadDetails.Clear();
        }

        public static void ClearCompletedDownloadDeatails()
        {
            for (var i = m_downloadDetails.Count - 1; i >= 0; i--)
            {
                if (m_downloadDetails[i].Progress.Equals("100%"))
                {
                    m_downloadDetails.RemoveAt(i);
                }
            }
        }

        public static void RetryDownloadDeatails()
        {
            for (var i = 0; i < m_downloadDetails.Count; i++)
            {
                if (m_downloadDetails[i].Progress.Length == 0 || m_downloadDetails[i].Progress.Equals("100%"))
                {
                    continue;
                }
                m_downloadDetails[i].Progress = string.Empty;
            }
        }
    }
}
