﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jira.SDK.Domain
{
	public class Project
	{
		private Jira _jira { get; set; }
		public Jira GetJira()
		{
			return _jira;
		}

		public void SetJira(Jira jira)
		{
			_jira = jira;
		}

		public Int32 ID { get; set; }
		public String Key { get; set; }
		public String Name { get; set; }

		private User _lead;

		public User Lead { get; set; }

		public User ProjectLead
		{
			get { return _lead ?? (_lead = _jira.Client.GetUser(Lead.Username)); }
		}

		private List<User> _assignableUsers;
		public List<User> AssignableUsers
		{
			get
			{
				return _assignableUsers ??
					   (_assignableUsers =
							_jira.Client.GetAssignableUsers(this.Key));
			}
		}

		private List<ProjectVersion> _projectVersions;
		public List<ProjectVersion> ProjectVersions
		{
			get
			{
				if (_projectVersions == null)
				{
					_projectVersions =
						   _jira.Client.GetProjectVersions(this.Key);

					_projectVersions.ForEach(vers => vers.Project = this);
				}
				return _projectVersions;
			}
		}

		public ProjectVersion PreviousVersion
		{
			get
			{
				return
					ProjectVersions.Where(vers => vers.ReleaseDate.CompareTo(DateTime.Now) <= 0)
						.OrderByDescending(vers => vers.ReleaseDate)
						.FirstOrDefault();
			}
		}

		public ProjectVersion CurrentVersion
		{
			get
			{
				return ProjectVersions.FirstOrDefault(
					vers => vers.StartDate.CompareTo(DateTime.Now) <= 0 && vers.ReleaseDate.CompareTo(DateTime.Now) > 0 && !vers.Archived);
			}
		}

		public ProjectVersion NextVersion
		{
			get
			{
				return ProjectVersions.Where(vers => vers.StartDate.CompareTo(DateTime.Now) > 0 && vers.ReleaseDate.CompareTo(DateTime.Now) > 0).OrderBy(vers => vers.StartDate).FirstOrDefault();
			}
		}

		public List<Epic> GetEpics()
		{
			List<Issue> epicIssues = _jira.Client.GetEpicIssuesFromProject(this.Name);
			epicIssues.ForEach(epic => epic.SetJira(this.GetJira()));

			List<Epic> epics = epicIssues.Select(epic => Epic.FromIssue(epic)).ToList();
			
			return epics.OrderBy(epic => epic.Rank).ToList();
		}

        public Epic GetEpic(String epicName)
        {
			Issue epicIssue = _jira.Client.GetEpicIssueFromProject(this.Name, epicName);
			epicIssue.SetJira(this.GetJira());
			return Epic.FromIssue(epicIssue);
        }

		public Epic GetEpicByKey(String issueKey)
		{
			Issue epicIssue = _jira.Client.GetIssue(issueKey);
			epicIssue.SetJira(this.GetJira());
			return Epic.FromIssue(epicIssue);
		}


		public Epic CreateEpic(String summary, IssueType type, User reporter)
		{
			Issue epic = new Issue()
			{
				Fields = new IssueFields()
				{
					Summary = summary,
					IssueType = type,
					Reporter = reporter
				}
			};

			GetJira().Client.AddIssue(epic);

			epic.Load();
			return Epic.FromIssue(epic);
		}

		public override int GetHashCode()
		{
			return this.Key.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return (obj is Project) && this.Key.Equals(((Project)obj).Key);
		}
	}
}