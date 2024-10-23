
namespace DevOpsInterface
{
    public class DevOpsFeatureToggle
    {
        private static Dictionary<string, bool> m_featureToggles = new Dictionary<string, bool>();

        public static DevOpsFeatureToggle Instance { get; } = new DevOpsFeatureToggle();

        private DevOpsFeatureToggle()
        {
            
        }

        public bool IsFeatureEnabled(string featureName)
        {
            if (m_featureToggles.ContainsKey(featureName))
            {
                return m_featureToggles[featureName];
            }

            return false;
        }   

        public void DisableFeature(string featureName)
        {
            m_featureToggles[featureName] = false;
        }

        public void EnableFeature(string featureName)
        {
            m_featureToggles[featureName] = true;
        }

        private void LoadFeatureToggles()
        {
            // Load feature toggles from configuration
        }

        public static bool IsEnabled(string featureName)
        {
            return Instance.IsFeatureEnabled(featureName);
        }
    }
}
