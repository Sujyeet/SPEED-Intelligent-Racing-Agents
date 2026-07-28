using UnityEngine;
using UnityEngine.UI;

namespace KartGame.UI
{
    public class HUDUI : MonoBehaviour
    {
        [SerializeField] private Text speedText;
        [SerializeField] private Text lapText;
        [SerializeField] private Text positionText;
        [SerializeField] private Image turboChargeFill;

        public void SetSpeed(float speed)
        {
            speedText.text = $"{(int)speed} km/h";
        }

        public void SetLap(int current, int total)
        {
            lapText.text = $"Lap {current}/{total}";
        }

        public void SetPosition(int position)
        {
            positionText.text = $"Pos: {position}";
        }

        public void SetTurboCharge(float charge)
        {
            turboChargeFill.fillAmount = charge;
        }
    }
}