using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Ink;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Newtonsoft.Json;

namespace vsInk
{
    public class AnnotationStore: IEnumerable<Annotation>
    {
        private readonly List<Annotation> annotations = new List<Annotation>();
        private readonly ITextDocument textDocument;
        private long id;
        private string filePath;

        public AnnotationStore(ITextDocument textDocument)
        {
            this.textDocument = textDocument;
            if (this.textDocument != null)
            {
                this.textDocument.FileActionOccurred += this.OnFileActionOccurred;
                this.filePath = this.textDocument.FilePath;
            }
        }

        public bool IsDirty { get; set; }

        public bool IsLoaded { get; private set; }

        public event EventHandler<AnnotationStoreChangedEventArgs> Changed;

        private void OnFileActionOccurred(object sender, TextDocumentFileActionEventArgs e)
        {
            switch (e.FileActionType)
            {
                case FileActionTypes.ContentSavedToDisk:
                    this.Save();
                    break;

                case FileActionTypes.DocumentRenamed:
                    var oldPath = this.filePath + ".vsInk";
                    this.filePath = e.FilePath;
                    var newPath = this.filePath + ".vsInk";
                    if (File.Exists(oldPath))
                    {
                        File.Move(oldPath, newPath);
                    }

                    break;
            }
        }


        public void Load(IWpfTextView view, GlobalSettings settings)
        {
            if (this.textDocument == null)
            {
                return;
            }

            var path = this.filePath + ".vsInk";
            if (!File.Exists(path))
            {
                this.IsDirty = false;
                this.IsLoaded = true;
                return;
            }

            using (var reader = File.OpenText(path))
            {
                var json = reader.ReadToEnd();
                var data = JsonConvert.DeserializeObject<DocumentData>(json);
                this.annotations.AddRange(data.Annotations.Select(a => a.ToModel(settings, view, this.id++)));
            }

            this.IsLoaded = true;
            this.IsDirty = false;
        }

        public void Save()
        {
            if (!this.IsDirty || (this.textDocument == null))
            {
                return;
            }

            var saveData = new DocumentData(this.annotations);
            var path = this.filePath + ".vsInk";
            using (var writer = File.CreateText(path))
            {
                var serializerSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented
                };
                var json = JsonConvert.SerializeObject(saveData, serializerSettings);
                writer.Write(json);
            }

            this.IsDirty = false;
        }

        public Annotation Add(AnnotationStroke originalStroke, AnnotationStroke cleanedStroke, InkDisplayMode displayMode, InkAdorner source)
        {
            var annotation = new Annotation(originalStroke, cleanedStroke, displayMode, this.id++);
            this.annotations.Add(annotation);
            this.IsDirty = true;
            this.Changed?.Invoke(this, new AnnotationStoreChangedEventArgs(source, ChangeType.Add));
            return annotation;
        }

        public void Remove(Stroke stroke, InkAdorner source)
        {
            var id = (long)stroke.GetPropertyData(Annotation.AnnotationID);
            var annotation = this.annotations.FirstOrDefault(a => a.Id == id);
            if (annotation != null)
            {
                this.IsDirty = true;
                this.annotations.Remove(annotation);
                this.Changed?.Invoke(this, new AnnotationStoreChangedEventArgs(source, ChangeType.Remove));
            }
        }

        public IEnumerator<Annotation> GetEnumerator()
        {
            return this.annotations.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.annotations.GetEnumerator();
        }
    }
}
