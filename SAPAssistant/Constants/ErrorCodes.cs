namespace SAPAssistant.Constants;

/// <summary>
/// Provides string constants for error and status codes used throughout the application.
/// The values match keys in localization resource files.
/// </summary>
public static class ErrorCodes
{
    public const string SESSION_TOKEN_NOT_FOUND = nameof(SESSION_TOKEN_NOT_FOUND);
    public const string SESSION_USER_NOT_FOUND = nameof(SESSION_USER_NOT_FOUND);
    public const string SESSION_REMOTE_IP_NOT_FOUND = nameof(SESSION_REMOTE_IP_NOT_FOUND);
    public const string CONNECTIONS_FETCH_ERROR = nameof(CONNECTIONS_FETCH_ERROR);
    public const string CONNECTIONS_FETCH_SUCCESS = nameof(CONNECTIONS_FETCH_SUCCESS);
    public const string NET_ERROR = nameof(NET_ERROR);
    public const string UNEXPECTED_ERROR = nameof(UNEXPECTED_ERROR);
    public const string SESSION_DATA_NOT_FOUND = nameof(SESSION_DATA_NOT_FOUND);
    public const string CONNECTION_FETCH_ERROR = nameof(CONNECTION_FETCH_ERROR);
    public const string EMPTY_RESPONSE = nameof(EMPTY_RESPONSE);
    public const string CONNECTION_FETCH_SUCCESS = nameof(CONNECTION_FETCH_SUCCESS);
    public const string UPDATE_CONNECTION_ERROR = nameof(UPDATE_CONNECTION_ERROR);
    public const string CONNECTION_UPDATED = nameof(CONNECTION_UPDATED);
    public const string CREATE_CONNECTION_ERROR = nameof(CREATE_CONNECTION_ERROR);
    public const string CONNECTION_CREATED = nameof(CONNECTION_CREATED);
    public const string VALIDATION_CONNECTION_ERROR = nameof(VALIDATION_CONNECTION_ERROR);
    public const string CONNECTION_VALID = nameof(CONNECTION_VALID);
    public const string CHAT_HISTORY_LOAD_ERROR = nameof(CHAT_HISTORY_LOAD_ERROR);
    public const string CHAT_FETCH_ERROR = nameof(CHAT_FETCH_ERROR);
    public const string CHAT_FETCH_SUCCESS = nameof(CHAT_FETCH_SUCCESS);
    public const string CHAT_SAVE_ERROR = nameof(CHAT_SAVE_ERROR);
    public const string CHAT_DELETE_ERROR = nameof(CHAT_DELETE_ERROR);
    public const string OK = nameof(OK);
    public const string SVC_UNAVAILABLE = nameof(SVC_UNAVAILABLE);
    public const string FE_NETWORK_HTTP = nameof(FE_NETWORK_HTTP);
    public const string FE_NETWORK_TIMEOUT = nameof(FE_NETWORK_TIMEOUT);
    public const string FE_NETWORK_ERROR = nameof(FE_NETWORK_ERROR);
    public const string GENERIC_ERROR = nameof(GENERIC_ERROR);
    public const string INTERNAL_ERROR = nameof(INTERNAL_ERROR);
    public const string LOGIN_SUCCESS = nameof(LOGIN_SUCCESS);
    public const string NEW_CHAT_TITLE = nameof(NEW_CHAT_TITLE);
}
