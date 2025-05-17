using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseController : MonoBehaviour
{
    private static PauseController instance;
    private static bool isGamePaused = false;

    // Static property để truy cập trạng thái pause từ bất kỳ script nào
    public static bool IsGamePaused
    {
        get { return isGamePaused; }
    }
    private void Start()
    {
        // Đảm bảo game bắt đầu ở trạng thái không pause
        SetPause(false);
        Debug.Log("Trạng thái pause ban đầu: " + isGamePaused);
    }

    // Thêm vào Update() của bất kỳ script nào đang hoạt động
    void Update()
    {
        // Nhấn phím R để reset trạng thái pause
        if (Input.GetKeyDown(KeyCode.R))
        {
            PauseController.SetPause(false);
            Debug.Log("Đã reset trạng thái pause: " + PauseController.IsGamePaused);
        }
    }
    private void Awake()
    {
        // Nếu chưa có instance
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Đảm bảo chỉ có duy nhất một instance
            Destroy(gameObject);
        }
    }

    // Phương thức static để thiết lập trạng thái pause
    public static void SetPause(bool pauseState)
    {
        isGamePaused = pauseState;

        // Thiết lập Time.timeScale để dừng/tiếp tục game
        Time.timeScale = pauseState ? 0 : 1;

        // Nếu bạn sử dụng các audio source, bạn có thể muốn dừng/tiếp tục âm thanh
        // Ví dụ: AudioListener.pause = pauseState;

        // Gọi event khi trạng thái pause thay đổi (nếu cần)
        if (instance != null)
        {
            instance.OnPauseStateChanged(pauseState);
        }
    }

    // Phương thức này có thể được override bởi các lớp con
    // hoặc được sử dụng để thông báo cho các component khác
    protected virtual void OnPauseStateChanged(bool isPaused)
    {
        // Có thể thêm logic xử lý khi trạng thái pause thay đổi
        Debug.Log("Game " + (isPaused ? "Paused" : "Resumed"));
    }

    // Phương thức tiện ích để toggle trạng thái pause
    public static void TogglePause()
    {
        SetPause(!isGamePaused);
    }
}