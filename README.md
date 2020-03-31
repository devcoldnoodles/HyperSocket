# HyperSocket
HyperSocket.Http는 간단하게 Http서버를 만들어주는 라이브러리입니다.
백문불여일견 예제로 설명드리겠습니다.
```
using System;
using HyperSocket.Http;

class Program
{
    public static void Main(string[] args)
    {
        // HttpListner는 클라이언트에게서 요청을 받고 처리합니다.
        HttpListener listener = new HttpListener();
        /*
            HttpRouter는 클라이언트의 요청을 어떻게 처리할 것인지 정의합니다.
            pattern은 클라이언트가 요청한 URL 주소와 매칭되는 정규표현식 패턴의 의미합니다.
            handle은 매칭되었을경우 실행되는 이벤트이며 req는 클라이언트의 요청정보를 담고있는 객체이며 res는 클라이언트의 요청에 대응되는 응답을 하기위한 객체입니다.
        */
        listener.Routers.Add(new HttpRouter
        {
            pattern = "^/", // 
            handle = (req, res) =>
            {
                res.Send("Hello"); // Send 함수는 인자로 넘겨지는 문자열을 클라이언트에게 응답할 수있도록 스트림을 설정합니다. 
                res.StatusCode = HttpStatusCode.OK; // StatusCode 멤버는 Default값으로 404(NotFound)로 설정되어있으므로 변경해주어야 합니다.
            }
        });
        // Start 함수는 인자로 넘겨지는 옵션값을 바탕으로 서버를 초기화하며 수신대기 상태로 넘어갑니다.
        listener.Start(new HttpOptions());
        while (listener.IsRunning) // IsRunning 속성은 서버가 수신대기 상태라면 true 아니면 false를 반환합니다.
        {
            if (Console.ReadLine() == "exit")
                listener.Stop(); // Stop함수는 수신대기 상태를 종료합니다.
        }
    }
}
}
```