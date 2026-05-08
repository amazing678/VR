using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class AutoFollowRay : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor currentRay;
    private int originalLayer;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        // ������ָ����ʱ�����̴���
        interactable.hoverEntered.AddListener(OnHoverEnter);
        originalLayer = gameObject.layer;
    }

    void OnHoverEnter(HoverEnterEventArgs args)
    {
        // ��������ǵ������ߣ��ҵ�ǰû���ڸ���
        if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor ray && currentRay == null)
        {
            currentRay = ray;

            // ����ħ��������ʼ�������һ�̣����Լ���ɡ��������ߡ��㣡
            // �������߾Ͳ��ᱻ�Լ���ס�����Ǵ�͸�Լ�ȥѰ��Զ���Ĳ�ۡ�
            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        }
    }

    void Update()
    {
        if (currentRay != null)
        {
            // ���ߴ���������ƽ���ط�������
            if (currentRay.TryGetHitInfo(out Vector3 hitPos, out Vector3 normal, out int posInLine, out bool isValid))
            {
                transform.position = Vector3.Lerp(transform.position, hitPos, Time.deltaTime * 15f);
            }
            else
            {
                // �������ָ��û�ж�������գ���������ǰ�� 2 �״�
                Vector3 skyPos = currentRay.transform.position + currentRay.transform.forward * 2f;
                transform.position = Vector3.Lerp(transform.position, skyPos, Time.deltaTime * 15f);
            }
        }
    }

    // ==========================================
    // �Զ�ƴװ�߼���������ײ����͸�����ʱ
    // ==========================================
    void OnTriggerEnter(Collider other)
    {
        // ���ײ���Ķ����������ǲ��Ǵ��� "socket" 
        // ���ҿ���� VRMedium ��ͼ���۽� socket1, socket2...��
        if (currentRay != null && other.name.ToLower().Contains("socket"))
        {
            currentRay = null; // ն��ǣ��������ֹͣ����

            // ˲���������ϲ������ۣ�
            transform.position = other.transform.position;
            transform.rotation = other.transform.rotation;

            // �ָ������㼶
            gameObject.layer = originalLayer;

            // �������������Խ� VRMedium �ı�ɫͨ���߼���
            // ����: VRMedium.instance.CheckWin();
        }
    }
}