using Our.Shield.Core.Enums;
using System;
using System.Text.RegularExpressions;
using System.Web;

namespace Our.Shield.Core.Models
{
    /// <summary>
    /// job interface
    /// </summary>
    public interface IJob
    {
        /// <summary>
        /// The Job Id
        /// </summary>
        Guid Key { get; }

        /// <summary>
        /// The Job Environment
        /// </summary>
        IEnvironment Environment { get; }

        /// <summary>
        /// The Job App Id
        /// </summary>
        IApp App { get; }

        /// <summary>
        /// Adds a Web Request to WebRequestsHandler collection
        /// </summary>
        /// <param name="stage">The stage of the pipeline to add a watch too</param>
        /// <param name="regex">The regex use to match for requests</param>
        /// <param name="priority">The priority of the request watch</param>
        /// <param name="request">The function to call when the regex matches a request</param>
        /// <returns></returns>
        int WatchWebRequests(
            PipeLineStage stage,
            Regex regex,
            int priority,
            Func<int, HttpApplication, WatchResponse> request);

        /// <summary>
        /// Removes a Web Requests from the WebRequestHandler collection
        /// </summary>
        /// <param name="stage">The stage of the pipeline to remove a watch from</param>
        /// <param name="regex">The regex of the corresponding Web Request to remove</param>
        /// <returns></returns>
        int UnwatchWebRequests(PipeLineStage stage, Regex regex);

        /// <summary>
        /// Removes all Web Requests from the WebRequestHandler collection created by this job
        /// </summary>
        /// <param name="stage">The stage of the pipeline to remove watches from</param>
        /// <returns></returns>
        int UnwatchWebRequests(PipeLineStage stage);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int UnwatchWebRequests();

        /// <summary>
        /// Removes all Web Requests from the WebRequestsHandler collection for the given App
        /// </summary>
        /// <param name="app">The App of the corresponding Web Requests to remove</param>
        /// <returns></returns>
        int UnwatchWebRequests(IApp app);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="regex"></param>
        /// <returns></returns>
        int ExceptionWebRequest(Regex regex);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        int ExceptionWebRequest(UmbracoUrl url);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="regex"></param>
        /// <returns></returns>
        int UnexceptionWebRequest(Regex regex);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        int UnexceptionWebRequest(UmbracoUrl url);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int UnexceptionWebRequest();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Regex PathToRegex(string path);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="regex"></param>
        /// <returns></returns>
        int IgnoreWebRequest(Regex regex);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        int IgnoreWebRequest(string path);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="regex"></param>
        /// <returns></returns>
        int UnignoreWebRequest(Regex regex);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        int UnignoreWebRequest(string path);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int UnignoreWebRequest();
    }
}
