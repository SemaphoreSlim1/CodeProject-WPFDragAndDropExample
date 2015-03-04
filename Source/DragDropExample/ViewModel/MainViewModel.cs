using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;

namespace DragDropExample.ViewModel
{

    public class User
    {        
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String EmailAddress { get; set; }
    }

    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {

        }

        #region FirstName Property

        /// <summary>
        /// Private member backing variable for <see cref="FirstName" />
        /// </summary>
        String _FirstName = null;

        /// <summary>
        /// Gets and sets the first name of the user
        /// </summary>
        public String FirstName
        {
            get
            {
                if (_FirstName == null)
                { _FirstName = String.Empty; }

                return _FirstName;
            }
            set { Set(() => FirstName, ref _FirstName, value); }
        }

        #endregion

        #region LastName Property

        /// <summary>
        /// Private member backing variable for <see cref="LastName" />
        /// </summary>
        private String _LastName = null;

        /// <summary>
        /// Gets and sets the last name of the user
        /// </summary>
        public String LastName
        {
            get
            {
                if (_LastName == null)
                { _LastName = String.Empty; }

                return _LastName;
            }
            set { Set(() => LastName, ref _LastName, value); }
        }

        #endregion
        
        #region EmailAddress Property

        /// <summary>
        /// Private member backing variable for <see cref="EmailAddress" />
        /// </summary>
        private String _EmailAddress = null;

        /// <summary>
        /// Gets and sets the email address of the user
        /// </summary>
        public String EmailAddress
        {
            get
            {
                if (_EmailAddress == null)
                { _EmailAddress = String.Empty; }

                return _EmailAddress;
            }
            set { Set(() => EmailAddress, ref _EmailAddress, value); }
        }

        #endregion

        #region Users Property

        /// <summary>
        /// Private member backing variable for <see cref="Users" />
        /// </summary>
        private ObservableCollection<User> _Users = null;

        /// <summary>
        /// Gets and sets the users we've added to the collection
        /// </summary>
        public ObservableCollection<User> Users
        {
            get
            {
                if (_Users == null)
                { _Users = new ObservableCollection<User>(); }

                return _Users;
            }
            set { Set(() => Users, ref _Users, value); }
        }

        #endregion

        #region AddUser Command

        /// <summary>
        /// Private member backing variable for <see cref="AddUser" />
        /// </summary>
        private RelayCommand _AddUser = null;

        /// <summary>
        /// Gets the command which adds a user to the collection of users
        /// </summary>
        public RelayCommand AddUser
        {
            get
            {
                if (_AddUser == null)
                { _AddUser = new RelayCommand(AddUser_Execute, AddUser_CanExecute); }

                return _AddUser;
            }
        }

        /// <summary>
        /// Implements the execution of <see cref="AddUser" />
        /// </summary>
        private void AddUser_Execute()
        {
            Users.Add(new User() { FirstName = FirstName, LastName = LastName, EmailAddress = EmailAddress });

            //clear out the properties
            FirstName = String.Empty;
            LastName = String.Empty;
            EmailAddress = String.Empty;
        }

        /// <summary>
        /// Determines if <see cref="AddUser" /> is allowed to execute
        /// </summary>
        private Boolean AddUser_CanExecute()
        {
            return String.IsNullOrWhiteSpace(FirstName) == false;
        }

        #endregion

    }
}