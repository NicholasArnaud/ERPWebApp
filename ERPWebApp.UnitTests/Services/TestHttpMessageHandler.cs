public class TestHttpMessageHandler : HttpMessageHandler
{
    private Action<HttpRequestMessage> requestMessageDelegate;
    private Dictionary<string, HttpResponseMessage> responseMessages;
    public Dictionary<string, object> CustomProperties { get; } = [];

    public TestHttpMessageHandler(Action<HttpRequestMessage>? requestMessage = null)
    {
        requestMessageDelegate = requestMessage ?? DefaultRequestHandler;
        responseMessages = [];
    }

    public void AddResponseMapping(string requestUrl, HttpResponseMessage responseMessage)
    {
        responseMessages.Add(requestUrl, responseMessage);
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage responseMessage;

        requestMessageDelegate(request);

        if (responseMessages.TryGetValue(request.RequestUri.ToString(), out responseMessage))
        {
            responseMessage.RequestMessage = request;
            return Task.FromResult(responseMessage);
        }

        // Add this block to handle the "/users/testUserId/sendMail" request  
        if (request.RequestUri.ToString().Contains("/users/testUserId/sendMail"))
        {
            responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
            responseMessage.RequestMessage = request;
            return Task.FromResult(responseMessage);
        }

        return Task.FromResult<HttpResponseMessage>(new HttpResponseMessage());
    }

    private void DefaultRequestHandler(HttpRequestMessage httpRequest)
    {

    }
}
