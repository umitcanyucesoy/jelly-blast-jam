using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Project.Scripts.Managers.Core;
using TMPro;
using UnityEngine;

namespace Project.Scripts.Core
{
    [SelectionBase]
    public class FailChecker : MonoBehaviour
    {
        [SerializeField] private Transform ballFloor;
        [SerializeField] private GameObject ballGroupFloor;

        [SerializeField] private int failOnCount = 100;
        [SerializeField] private SkinnedMeshRenderer failBarRenderer;
        [SerializeField] private SkinnedMeshRenderer failBarFloorRenderer;
        [SerializeField] private MeshCollider meshCollider;
        [SerializeField] private List<Rigidbody> cellPiecesRb;
        

        private List<Ball> balls = new List<Ball>();
        private Color defaultColor;
        [SerializeField] private Color targetColor;
        private Vector3 failTransformBeginPosition;
        private Vector3 failTransformEndPosition;
        [SerializeField] private Vector3 textBeginPosition;
        [SerializeField] private Vector3 textEndPosition;
        [SerializeField] private Vector3 transformLerpBegin;
        [SerializeField] private Vector3 transformLerpEnd;
        
        [SerializeField] private Canvas canvas;
        [SerializeField] private TMP_Text countText;
        [SerializeField] private List<TMP_Text> dangerTexts;
        [SerializeField] private TMP_Text timerText;
        
        [SerializeField] private RectTransform heartIcon;

        private float fontSize = 20f;

        private bool isFailed;
        private Mesh bakedMesh;
        private AudioSource source;

        private void Start()
        {
            failTransformBeginPosition = ballFloor.position;
            failTransformEndPosition = ballFloor.position - Vector3.forward * 0.5f;
            GM.Instance.currentLevel.failChecker = this;
            failOnCount = GM.Instance.currentLevel.counterMax;
            defaultColor = failBarRenderer.material.color;
            countText.text = $"{failOnCount}";
            fontSize = countText.fontSize;
            bakedMesh = new Mesh();
            source = GetComponent<AudioSource>();
            var sound = AudioManager.Instance.sounds.First(s => s.name == "Clock");
            source.volume = source.volume;
            source.clip = sound.clip;
        }

        void LateUpdate()
        {
            //if (isFailed) return;
            if (failBarRenderer == null || meshCollider == null)
                return;

            failBarRenderer.BakeMesh(bakedMesh);
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = bakedMesh;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isFailed) return;

            if (other.TryGetComponent(out Ball ball) && !balls.Contains(ball))
            {
                ball.myGroup.ReleaseTension();
                balls.Add(ball);
                ValueLerp();
                if (balls.Count >= failOnCount)
                {
                    StartCoroutine(FailDelayer());
                }
            }
        }

        private IEnumerator FailDelayer()
        {
            isFailed = true;
            var elapsedTime = 0f;
            var totalTime = 3f;

            yield return new WaitForSeconds(0.5f);
            
            if (CheckBreak()) yield break;
            
            timerText.text = "3";
            DOTween.Kill("Timer", false);
            var timerSeq = DOTween.Sequence().SetId("Timer").SetDelay(0.5f);
            timerSeq.Join(heartIcon.DOScale(0f, 0.5f));
            timerSeq.Join(countText.DOScale(0f, 0.5f));
            timerSeq.Join(timerText.DOScale(1f, 0.5f));
            //timerSeq.Append(DOVirtual.Int(3,0, 3f, value => timerText.text = $"{value}"));
            //timerSeq.Join(timerText.DOColor(Color.red, 0.5f).SetLoops(6, LoopType.Yoyo));
            //timerSeq.Join(timerText.DOScale(1.25f, 0.5f).SetLoops(6, LoopType.Yoyo));
            
            dangerTexts[0].gameObject.SetActive(true);
            dangerTexts[1].gameObject.SetActive(true);
            dangerTexts[0].transform.localScale = Vector3.one * 0.01f;
            dangerTexts[1].transform.localScale = Vector3.one * 0.01f;
            dangerTexts[0].transform.DOScale(0.75f, 0.5f).SetEase(Ease.Linear).SetId("FailSeq");
            yield return dangerTexts[1].transform.DOScale(0.75f, 0.5f).SetEase(Ease.Linear).SetId("FailSeq").WaitForCompletion();

            if (CheckBreak()) yield break;
            
            dangerTexts[0].DOScale(1.3f, 0.5f).SetEase(Ease.Linear).SetLoops(7, LoopType.Yoyo).SetId("FailSeq");
            dangerTexts[1].DOScale(1.3f, 0.5f).SetEase(Ease.Linear).SetLoops(7, LoopType.Yoyo).SetId("FailSeq");
            
            DOVirtual.DelayedCall(0.4f, () =>
            {
                source.Stop();
                source.Play();
            }).SetId("FailSeq");
            DOVirtual.DelayedCall(1.4f, () =>
            {
                source.Stop();
                source.Play();
            }).SetId("FailSeq");
            DOVirtual.DelayedCall(2.4f, () =>
            {
                source.Stop();
                source.Play();
            }).SetId("FailSeq");
            
            DOVirtual.DelayedCall(0.9f, () =>
            {
                timerText.text = "2";
                timerText.DOScale(1.25f, 0.1f).SetLoops(2,LoopType.Yoyo).SetId("FailSeq");
            }).SetId("FailSeq");
            
            DOVirtual.DelayedCall(1.9f, () =>
            {
                timerText.text = "1";
                timerText.DOScale(1.25f, 0.1f).SetLoops(2, LoopType.Yoyo).SetId("FailSeq");
            }).SetId("FailSeq");

            DOVirtual.Float(100f, 105f, 0.5f, value =>
            {
                failBarFloorRenderer.SetBlendShapeWeight(0, value);
                failBarRenderer.SetBlendShapeWeight(0, value);
            }).SetLoops(14, LoopType.Yoyo).SetId("FailSeq");

            while (balls.Count >= failOnCount)
            {
                var progress = Mathf.Clamp01(elapsedTime / totalTime);
                if (progress >= 1f)
                {
                    GM.Instance.Lost();
                    AudioManager.Instance.PlayAudio("WallBreak");
                    failBarRenderer.gameObject.SetActive(false);
                    ballFloor.gameObject.SetActive(false);
                    ballGroupFloor.SetActive(false);
                    ballFloor.gameObject.SetActive(false);
                    foreach (var rigidbody1 in cellPiecesRb)
                    {
                        rigidbody1.gameObject.SetActive(true);
                    }

                    DOTween.Kill("TextSequence");
                    DOTween.Kill("FailSeq");
                    foreach (var dangerText in dangerTexts)
                    {
                        dangerText.DOScale(0f, 0.25f).OnComplete((() => dangerText.gameObject.SetActive(false)));
                    }

                    canvas.transform.DOScale(0f, 0.5f);
                    //countText.DOFontSize(0f, 0.5f);
                    //heartIcon.DOScale(0f, 0.5f);

                    yield break;
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            CheckBreak();
        }

        private bool CheckBreak()
        {
            if (balls.Count < failOnCount)
            {
                Debug.Log("Reset");
                source.Stop();
                DOTween.Kill("Timer", false);
                DOTween.Kill("FailSeq", false);
                dangerTexts[0].transform.DOScale(0.01f, 0.25f).SetEase(Ease.Linear).OnComplete((() => dangerTexts[0].gameObject.SetActive(false)));
                dangerTexts[1].transform.DOScale(0.01f, 0.25f).SetEase(Ease.Linear).OnComplete((() => dangerTexts[1].gameObject.SetActive(false)));
                
                var timerSeq = DOTween.Sequence().SetId("Timer");
                timerSeq.Join(heartIcon.DOScale(1.5f, 0.1f));
                timerSeq.Join(countText.DOScale(1f, 0.1f));
                timerSeq.Join(timerText.DOScale(0f, 0.1f));
                //timerSeq.Append(DOVirtual.Int(3,0, 3f, value => timerText.text = $"{value}"));
                timerSeq.Join(timerText.DOColor(Color.white, 0.1f));

                isFailed = false;
                return true;
            }

            return false;
        }

        private void ValueLerp()
        {
            countText.text = $"{Mathf.Max(0, failOnCount - balls.Count)}";
            DOTween.Kill("TextSequence", true);
            var seq = DOTween.Sequence().SetId("TextSequence");
            seq.Join(countText.DOColor(Color.red, 0.125f).SetLoops(2, LoopType.Yoyo));
            if (heartIcon.transform.localScale == Vector3.one * 1.5f)
            {
                seq.Join(heartIcon.DOPunchScale(Vector3.one * 0.25f, 0.125f));
            }
            
            var ratio = ((float)balls.Count / failOnCount);
            failBarFloorRenderer.SetBlendShapeWeight(0, ratio * 100f);
            failBarRenderer.SetBlendShapeWeight(0, ratio * 100f);
            failBarRenderer.BakeMesh(bakedMesh);
            var transformLerp = Vector3.Lerp(transformLerpBegin, transformLerpEnd, ratio);
            var lerp = Vector3.Lerp(failTransformBeginPosition, failTransformEndPosition, ratio);
            var textLerp = Vector3.Lerp(textBeginPosition, textEndPosition, ratio);
            transform.position = transformLerp;
            canvas.transform.position = textLerp;
            ballFloor.position = lerp;
            failBarRenderer.material.color = Color.Lerp(defaultColor, targetColor, ratio);
        }

        public void UpdateOnBallDestroyed(Ball ball)
        {
            if (balls.Contains(ball))
            {
                balls.Remove(ball);
                ValueLerp();
            }
        }
    }
}