namespace HyperSocket.Http
{
    public enum HttpMethod : byte
    {
        Delete, // HTTP DELETE 프로토콜 메서드를 나타냅니다.
        Get, //HTTP GET 프로토콜 메서드를 나타냅니다.
        Head, // HTTP HEAD 프로토콜 메서드를 나타냅니다. HEAD 메서드는 서버에서 응답의 메시지 본문 없이 메시지 헤더만 반환한다는 점을 제외하고는 GET 메서드와 동일합니다.
        Options, //HTTP OPTIONS 프로토콜 메서드를 나타냅니다.
        Post, //새 엔터티를 URI에 추가할 항목으로 게시하는 데 사용되는 HTTP POST 프로토콜 메서드를 나타냅니다.
        Put, // URI로 식별된 엔터티를 바꾸는 데 사용되는 HTTP PUT 프로토콜 메서드를 나타냅니다.
        Trace, //HTTP TRACE 프로토콜 메서드를 나타냅니다.
    }
}