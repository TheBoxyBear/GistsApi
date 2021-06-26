using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GistsApi
{
    /*
     * JSON of gist object
     *
     {
    "url": "https://api.github.com/gists/5f774909d8162f5b75f6",
    "id": "1",
    "description": "description of gist",
    "public": true,
    "user": {
      "login": "octocat",
      "id": 1,
      "avatar_url": "https://github.com/images/error/octocat_happy.gif",
      "gravatar_id": "somehexcode",
      "url": "https://api.github.com/users/octocat"
    },
    "files": {
      "ring.erl": {
        "size": 932,
        "filename": "ring.erl",
        "raw_url": "https://gist.github.com/raw/365370/8c4d2d43d178df44f4c03a7f2ac0ff512853564e/ring.erl"
      }
    },
    "comments": 0,
    "comments_url": "https://api.github.com/gists/a9416aa3c9ea9e54519c/comments/",
    "html_url": "https://gist.github.com/1",
    "git_pull_url": "git://gist.github.com/1.git",
    "git_push_url": "git@gist.github.com:1.git",
    "created_at": "2010-04-14T02:15:15Z",
    "forks": [
      {
        "user": {
          "login": "octocat",
          "id": 1,
          "avatar_url": "https://github.com/images/error/octocat_happy.gif",
          "gravatar_id": "somehexcode",
          "url": "https://api.github.com/users/octocat"
        },
        "url": "https://api.github.com/gists/e0da84b28ced9820b3ea",
        "created_at": "2011-04-14T16:00:49Z"
      }
    ],
    "history": [
      {
        "url": "https://api.github.com/gists/8d67e15b854be62107ba",
        "version": "57a7f021a713b1c5a6a199b54cc514735d2d462f",
        "user": {
          "login": "octocat",
          "id": 1,
          "avatar_url": "https://github.com/images/error/octocat_happy.gif",
          "gravatar_id": "somehexcode",
          "url": "https://api.github.com/users/octocat"
        },
        "change_status": {
          "deletions": 0,
          "additions": 180,
          "total": 180
        },
        "committed_at": "2010-04-14T02:15:15Z"
      }
    ]
  }

    */
    public class GistObject
  {
    public string url { set; get; }
    public string id { set; get; }
    public string description { set; get; }
    public bool @public { set; get; }
    public User user { set; get; }
    public Files files { set; get; }
    public double comments { set; get; }
    public string comments_url { set; get; }
    public string html_url { set; get; }
    public string git_pull_url { set; get; }
    public string git_push_url { set; get; }
    public string created_at { set; get; }
    public Fork[] forks { get; set; }
    public History[] history { get; set; }
    public GistObject()
    {
      url = string.Empty;
      description = string.Empty;
      user = new User();
      files = new Files();
      html_url = string.Empty;
      git_pull_url = string.Empty;
      git_push_url = string.Empty;
      created_at = string.Empty;
      forks = Array.Empty<Fork>();
      history = Array.Empty<History>();
    }

  }

  public class User
  {
    public string login { set; get; }
    public string id { set; get; }
    public string avatar_url { set; get; }
    public string gravatar_id { set; get; }
    public string url { set; get; }
    public User()
    {
      login = string.Empty;
      avatar_url = string.Empty;
      gravatar_id = string.Empty;
      url = string.Empty;
    }
  }

    public class Files : Collection<File>
  {
    public Files()
      : base() { }

    public Files(IList<File> collection)
      : base(collection) { }

  }

  public class File
  {
    public double size { set; get; }
    public string filename { set; get; }
    public string raw_url { set; get; }
    public File()
    {
      filename = string.Empty;
      raw_url = string.Empty;
    }
  }

  public class Fork
  {
    public User user { set; get; }
    public string url { set; get; }
    public string created_at { set; get; }
    public Fork()
    {
      url = string.Empty;
      created_at = string.Empty;
      user = new User();
    }
  }
  public class History
  {
    public string url { set; get; }
    public string version { set; get; }
    public User user { set; get; }
    public ChangeStatus change_status { set; get; }
    public string committed_at { set; get; }
    public History()
    {
      url = string.Empty;
      version = string.Empty;
      user = new User();
      change_status = new ChangeStatus();
      committed_at = string.Empty;
    }
  }
  public class ChangeStatus
  {
    public double deletions { set; get; }
    public double additions { set; get; }
    public double total { set; get; }
    public ChangeStatus()
    {}
  }

}
