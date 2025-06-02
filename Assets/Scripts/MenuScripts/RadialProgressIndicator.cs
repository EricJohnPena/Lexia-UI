using UnityEngine;
using UnityEngine.UI;

namespace MenuScripts
{
    public class RadialProgressIndicator : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private Text percentageText;
        [SerializeField] private Text subjectNameText;
        [SerializeField] private Text generatedCommentText;
        [SerializeField] private Text teacherCommentText;
        
        private float targetFill = 0f;
        private float currentFill = 0f;
        private const float FILL_SPEED = 5f;

        private void Update()
        {
            if (currentFill != targetFill)
            {
                currentFill = Mathf.MoveTowards(currentFill, targetFill, Time.deltaTime * FILL_SPEED);
                fillImage.fillAmount = currentFill;
                percentageText.text = $"{Mathf.RoundToInt(currentFill * 100)}%";
            }
        }

        public void SetProgress(float progress, string subjectName)
        {
            targetFill = Mathf.Clamp01(progress);
            subjectNameText.text = subjectName;
        }

        public void SetComments(string generatedComment, string teacherComment)
        {
            if (generatedCommentText != null)
            {
                generatedCommentText.text = generatedComment;
            }

            if (teacherCommentText != null)
            {
                teacherCommentText.text = teacherComment;
            }
        }
    }
} 