using System.Collections.Generic;
using System.Linq;

using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Archiving;
using Sitecore.Data.Items;
using Sitecore.Globalization;

namespace Examples.ArchiveVersionsAgent.Tasks
{
    public class ArchiveVersionsAgentTask
    {
        private readonly Stack<Item> _items = new Stack<Item>();

        private readonly Database _master;

        private readonly Archive _archive;

        public ArchiveVersionsAgentTask()
        {
            _master = Factory.GetDatabase("master");
            _archive = ArchiveManager.GetArchive("archive", _master);
        }

        public string Root { get; set; }

        public int MaxVersions { get; set; }

        public void Run()
        {
            Item rootItem = ID.IsID(Root) ? _master.GetItem(new ID(Root)) : _master.GetItem(Root);
            if (rootItem != null)
            {
                _items.Push(rootItem);
                ArchiveVersions();
            }
        }

        private void ArchiveVersions()
        {
            while (_items.Count > 0)
            {
                Item currentItem = _items.Pop();
                ArchiveVersionsForItem(currentItem);

                foreach (Item child in currentItem.Children)
                {
                    _items.Push(child);
                }
            }
        }

        private void ArchiveVersionsForItem(Item item)
        {
            foreach (Language lang in item.Languages)
            {
                Item itemInLang = _master.GetItem(item.ID, lang);
                if (itemInLang.Versions.Count > MaxVersions)
                {
                    Item[] versions = itemInLang.Versions.GetVersions();
                    int versionsToArchive = versions.Length - MaxVersions;
                    foreach (Item versionToArchive in versions.Take(versionsToArchive))
                    {
                        _archive.ArchiveVersion(versionToArchive);
                    }
                }
            }
        }
    }
}