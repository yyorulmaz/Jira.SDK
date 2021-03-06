﻿using Jira.SDK.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jira.SDK
{
    public class Jira
    {
        private IJiraClient _client;
		internal IJiraClient Client { get { return _client; } }

		public List<Field> Fields { get; private set; }

		public void Connect(IJiraClient client)
		{
			_client = client;
		}

        public void Connect(String url, String username, String password)
        {
            Connect(new JiraClient(url, username, password));
        }

        public List<Project> GetProjects()
        {
			List<Project> projects = _client.GetProjects();
            projects.ForEach(project => project.SetJira(this));
            return projects;
        }

        public Project GetProject(String key)
        {
            Project project = _client.GetProject(key);
            if (project != null)
            {
                project.SetJira(this);
            }
            return project;
        }

		public Issue GetIssue(String key)
		{
			Issue issue = _client.GetIssue(key);
            if(String.IsNullOrEmpty(issue.Key))
            {
                return null;
            }
			if (issue != null)
			{
				issue.SetJira(this);
			}

			return issue;
		}

        public List<Issue> SearchIssues(String jql)
        {
            List<Issue> issues = Client.SearchIssues(jql);
            issues.ForEach(issue => issue.SetJira(this));
            return issues;
        }

		public List<AgileBoard> GetAgileBoards()
		{
			List<AgileBoard> boards = _client.GetAgileBoards();
			boards.ForEach(board => board.SetJira(this));
			return boards;
		}

		public AgileBoard GetAgileBoard(Int32 agileBoardID)
		{
			AgileBoard board = GetAgileBoards().Where(b => b.ID == agileBoardID).FirstOrDefault();
			if (board == null)
			{
				throw new Exception(String.Format("The board with ID {0} does not exist", agileBoardID));
			}

			return board;
		}

        public List<IssueFilter> GetFilters()
        {
            List<IssueFilter> filters = _client.GetFavoriteFilters();
            filters.ForEach(filter => filter.SetJira(this));
            return filters;
        }

		public IssueFilter GetFilter(String filtername)
		{
			IssueFilter filter = GetFilters().Where(f => f.Name.Equals(filtername)).FirstOrDefault();
			if (filter == null)
			{
				throw new Exception(String.Format("The filter with name {0} does not exist", filtername));
			}

			return filter;
		}
    }
}
