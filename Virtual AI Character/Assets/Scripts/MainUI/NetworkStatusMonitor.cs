using UnityEngine;
using UnityEngine.UI;

public class NetworkStatusMonitor : MonoBehaviour
{
    public Image networkIcon;
    public Color connectedColor = Color.green;
    public Color disconnectedColor = Color.red;
    public Color connectingColor = Color.yellow;

    void Update()
    {
        switch (Application.internetReachability)
        {
            case NetworkReachability.NotReachable:
                SetIconColor(disconnectedColor);
                break;
            case NetworkReachability.ReachableViaCarrierDataNetwork:
                SetIconColor(connectedColor);
                break;
            case NetworkReachability.ReachableViaLocalAreaNetwork:
                SetIconColor(connectedColor);
                break;
            default:
                SetIconColor(connectingColor);
                break;
        }
    }

    private void SetIconColor(Color color)
    {
        if (networkIcon != null)
        {
            networkIcon.color = color;
        }
    }
}