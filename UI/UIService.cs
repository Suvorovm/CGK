using System.Collections.Generic;
using System.Linq;
using CommonGameKit.com.suvorov.common_e419b53c39.Utils;
using Core.UI.Model;
using CommonGameKit.Utils;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    public class UIService : MonoBehaviour
    {
        private const string DIALOG_CONTAINER = "DialogContainer";
        private const string SCREEN_CONTAINER = "ScreenContainer";
        private readonly List<DialogData> _dialogs = new List<DialogData>();

        private GameObject _screenContainer;
        private GameObject _dialogContainer;

        private DialogData _currentDialog;
        private MonoBehaviour _currentScreen;
        private int _maxSortingOrder;

        [PublicAPI]
        public void Init()
        {
            ValidateUIStruct();
        }

        [PublicAPI]
        public T ShowScreen<T>(string prefabPath, [CanBeNull] IScreenModel model = null)
            where T : MonoBehaviour, IScreen
        {
            T screenMonoBehaviour = InstantiateUI<T>(prefabPath);

            ClearScreenAndDialogs();
            screenMonoBehaviour.transform.SetParent(_screenContainer.transform, false);

            screenMonoBehaviour.Configure(model);
            _currentScreen = screenMonoBehaviour;
            return screenMonoBehaviour;
        }


        [PublicAPI]
        public T ShowDialog<T>(string prefabPath, [CanBeNull] IDialogModel model = null)
            where T : MonoBehaviour, IDialog
        {
            DialogData existingDialog = GetDialogData<T>();
            if (existingDialog != null)
            {
                PushUpExistingDialog(existingDialog);
                return existingDialog.MonoBehaviour.GetComponent<T>();
            }

            T dialog = InstantiateUI<T>(prefabPath);
            DialogData configuredDialog = ConfigureDialog(dialog, model);

            _dialogs.Add(configuredDialog);
            _currentDialog = configuredDialog;

            return dialog;
        }

        [PublicAPI]
        public void HideDialog(GameObject dialogToHide)
        {
            DialogData dialogData = GetDialogData(dialogToHide);
            if (dialogData == null)
            {
                Debug.LogWarning("Dialog already hide or not exists");
                return;
            }

            RemoveDialog(dialogData);
        }

        [PublicAPI]
        public void HideAllDialogs()
        {
            for (int i = 0; i < _dialogs.Count; i++)
            {
                Destroy(_dialogs[i].MonoBehaviour.gameObject);
            }

            _currentDialog = null;
            _maxSortingOrder = 0;
            _dialogs.Clear();
        }

        [PublicAPI]
        public void HideDialog<T>()
            where T : MonoBehaviour, IDialog
        {
            DialogData dialogData = GetDialogData<T>();
            if (dialogData == null)
            {
                Debug.LogWarning("Dialog already hide or not exists");
                return;
            }

            RemoveDialog(dialogData);
        }


        [PublicAPI]
        public bool HasDialog<T>() where T : MonoBehaviour, IDialog
        {
            DialogData dialog = GetDialogData<T>();
            return dialog != null;
        }

        [PublicAPI]
        public void HideScreen()
        {
            _screenContainer.gameObject.RemoveAllChild();
        }

        [PublicAPI]
        private IScreen GetCurrentScreen()
        {
            return _currentScreen == null ? null : _currentScreen.GetComponent<IScreen>();
        }

        private void ValidateUIStruct()
        {
            _dialogContainer = gameObject.GetChildByName(DIALOG_CONTAINER);
            _screenContainer = gameObject.GetChildByName(SCREEN_CONTAINER);
            Predications.CheckNotNull(_dialogContainer);
            Predications.CheckNotNull(_screenContainer);
        }

        private T InstantiateUI<T>(string prefabPath)
            where T : MonoBehaviour
        {
            GameObject prefab = Resources.Load<GameObject>(prefabPath);
            GameObject instantiatedScreen = Instantiate(prefab);
            T screenMonoBehaviour = instantiatedScreen.GetComponent<T>();
            return screenMonoBehaviour;
        }

        [CanBeNull]
        private DialogData GetDialogData<T>() where T : MonoBehaviour, IDialog
        {
            DialogData dialog = _dialogs.FirstOrDefault(d => d.MonoBehaviour.GetComponent<T>() != null);
            return dialog;
        }

        [CanBeNull]
        private DialogData GetDialogData(GameObject dialogObject)
        {
            DialogData dialog = _dialogs.FirstOrDefault(d => d.MonoBehaviour.gameObject == dialogObject);
            return dialog;
        }

        private DialogData ConfigureDialog<T>(T dialog, [CanBeNull] IDialogModel model = null)
            where T : MonoBehaviour, IDialog
        {
            dialog.Configure(model);
            Canvas canvas = dialog.gameObject.GetOrCreateComponent<Canvas>();
            dialog.gameObject.GetOrCreateComponent<GraphicRaycaster>();
            dialog.gameObject.transform.SetParent(_dialogContainer.transform, false);
            _maxSortingOrder++;
            canvas.overrideSorting = true;
            canvas.sortingOrder = _maxSortingOrder;
            DialogData dialogData = new DialogData()
            {
                Order = _maxSortingOrder,
                MonoBehaviour = dialog,
                Canvas = canvas
            };
            return dialogData;
        }

        private void ClearScreenAndDialogs()
        {
            _screenContainer.gameObject.RemoveAllChild();
            _dialogContainer.gameObject.RemoveAllChild();
            _currentDialog = null;
            _maxSortingOrder = 0;
            _dialogs.Clear();
        }

        private void PushUpExistingDialog(DialogData existingDialog)
        {
            for (int i = 0; i < _dialogs.Count; i++)
            {
                if (_dialogs[i].Order > existingDialog.Order)
                {
                    _dialogs[i].Order--;
                    _dialogs[i].Canvas.sortingOrder--;
                }
            }

            existingDialog.Order = _maxSortingOrder;
            existingDialog.Canvas.sortingOrder = _maxSortingOrder;
            _currentDialog = existingDialog;
        }

        private void RemoveDialog(DialogData removingDialog)
        {
            for (int i = 0; i < _dialogs.Count; i++)
            {
                if (_dialogs[i].Order > removingDialog.Order)
                {
                    _dialogs[i].Order--;
                    _dialogs[i].Canvas.sortingOrder--;
                }
            }

            _maxSortingOrder--;
            _dialogs.Remove(removingDialog);
            if (_dialogs.Count == 0)
            {
                Destroy(removingDialog.MonoBehaviour.gameObject);
                _currentDialog = null;
                return;
            }

            if (removingDialog == _currentDialog)
            {
                DialogData topDialog = _dialogs.First(d => d.Order == _maxSortingOrder);
                _currentDialog = topDialog;
            }

            Destroy(removingDialog.MonoBehaviour.gameObject);
        }

        private class DialogData
        {
            public MonoBehaviour MonoBehaviour;
            public Canvas Canvas;
            public int Order;
        }
    }
}