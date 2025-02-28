﻿using App1.Database;
using App1.Interfaces;
using App1.Models;
using App1.Services;
using App1.Views;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace App1.ViewModels
{
    public class GameViewModel : BaseViewModel
    {
        private static GameViewModel _instance = null;

        private static CubeInfo[,] _cubesInfo = new CubeInfo[4, 4];

        private string _currentScore = "0";

        public static bool IsInitialized = false;

        private const int _ROWS_AMOUNT = 4;
        private const int _COLUMNS_AMOUNT = 4;

        private static Random _random = new Random();
        private static Grid _playField;

        private static ObservableCollection<Button> _playGrid { get; set; } = new ObservableCollection<Button>();
        public static ObservableCollection<Button> PlayGrid
        {
            get => _playGrid;
            set => _playGrid = value;
        }

        public string CurrentScore
        {
            get => _currentScore;
            set
            {
                _currentScore = value;
                OnPropertyChanged(nameof(CurrentScore));
            }
        }

        public ICommand GameOverCommand { get; }
        public ICommand PressButtonCommand { get; }

        public static GameViewModel Create(Grid playField)
        {
            if (_instance is null)
            {
                _instance = new GameViewModel(playField);
            }
            return _instance;
        }

        private GameViewModel(Grid playField)
        {
            _playField = playField;
            FillGrid();
            IsInitialized = true;
            GameOverCommand = new Command(OnGameOverClicked);
        }

        public void FillGrid()
        {
            try
            {
                bool check = false;
                int randomValue = _random.Next(0, 15);
                int counter = 0;
                for (int i = 0; i < _ROWS_AMOUNT; i++)
                {
                    for (int j = 0; j < _COLUMNS_AMOUNT; j++)
                    {
                        _cubesInfo[i, j] = new CubeInfo();

                        Button newCube = new Button();

                        if (counter == randomValue && !check)
                        {
                            check = true;
                            newCube.BackgroundColor = Color.Transparent;
                        }
                        else
                        {
                            newCube.Text = (++counter).ToString();
                            newCube.FontSize = 35;
                            newCube.TextColor = Color.Black;
                            newCube.Clicked += ShiftCube;

                            _cubesInfo[i, j].Text = newCube.Text;
                        }
                        _playGrid.Add(newCube);
                    }
                }

                MixCollection(_playGrid);
                AddCubesToGrid();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void AddCubesToGrid()
        {
            try
            {
                int counter = 0;
                for (int i = 0; i < _ROWS_AMOUNT; i++)
                {
                    for (int j = 0; j < _COLUMNS_AMOUNT; j++)
                    {
                        _cubesInfo[i, j].Y = i;
                        _cubesInfo[i, j].X = j;

                        if (!string.IsNullOrWhiteSpace(_cubesInfo[i, j].Text))
                        {
                            _cubesInfo[i, j].IsFree = false;
                        }
                        _playField.Children.Add(_playGrid[counter++], j, i);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private static void RefreshCubes()
        {
            try
            {
                _playField.Children.Clear();

                int counter = 0;
                for (int i = 0; i < _ROWS_AMOUNT; i++)
                {
                    for (int j = 0; j < _COLUMNS_AMOUNT; j++)
                    {
                        _playField.Children.Add(_playGrid[counter++], j, i);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void MixCollection(ObservableCollection<Button> collection)
        {
            try
            {
                for (int i = collection.Count - 1; i >= 1; i--)
                {
                    int j = _random.Next(i + 1);
                    // wrap values data[j] and data[i]
                    var temp = collection[j];
                    collection[j] = collection[i];
                    collection[i] = temp;

                    CubeInfo tempInfo = _cubesInfo[j / _COLUMNS_AMOUNT, j % _COLUMNS_AMOUNT];
                    CubeInfo cube = new CubeInfo(tempInfo.Text, tempInfo.X, tempInfo.Y, tempInfo.IsFree);
                    _cubesInfo[j / _COLUMNS_AMOUNT, j % _COLUMNS_AMOUNT] = _cubesInfo[i / _COLUMNS_AMOUNT, i % _COLUMNS_AMOUNT];
                    _cubesInfo[i / _COLUMNS_AMOUNT, i % _COLUMNS_AMOUNT] = cube;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void ShiftCube(object sender, EventArgs e)
        {
            try
            {
                // explicit cast from RoutedEventArgs to Button
                Button button = (Button)sender;

                for (int i = 0; i < _ROWS_AMOUNT; i++)
                {
                    for (int j = 0; j < _COLUMNS_AMOUNT; j++)
                    {
                        if (_cubesInfo[i, j].Text == button.Text)
                        {
                            if (i == 0)
                            {
                                if (j == 0)
                                {
                                    if (_cubesInfo[i, j + 1].IsFree)
                                    {
                                        WrapCubes(_cubesInfo[i, j + 1], (i, j), (i, j + 1));
                                    }
                                    else if (_cubesInfo[i + 1, j].IsFree)
                                    {
                                        WrapCubes(_cubesInfo[i + 1, j], (i, j), (i + 1, j));
                                    }
                                }
                                else if (j == 3)
                                {
                                    if (_cubesInfo[i, j - 1].IsFree)
                                    {
                                        WrapCubes(_cubesInfo[i, j - 1], (i, j), (i, j - 1));
                                    }
                                    else if (_cubesInfo[i + 1, j].IsFree)
                                    {
                                        WrapCubes(_cubesInfo[i + 1, j], (i, j), (i + 1, j));
                                    }
                                }
                                else
                                {
                                    if (_cubesInfo[i + 1, j].IsFree)
                                    {
                                        WrapCubes(_cubesInfo[i + 1, j], (i, j), (i + 1, j));
                                    }
                                    else if (_cubesInfo[i, j + 1].IsFree)
                                    {
                                        WrapCubes(_cubesInfo[i, j + 1], (i, j), (i, j + 1));
                                    }
                                    else if (_cubesInfo[i, j - 1].IsFree)
                                    {
                                        WrapCubes(_cubesInfo[i, j - 1], (i, j), (i, j - 1));
                                    }
                                }
                            }
                            else if (i == 3)
                            {
                                if (j == 0)
                                {
                                    if (_cubesInfo[i, j + 1].IsFree)
                                    {
                                        WrapCubes(_cubesInfo[i, j + 1], (i, j), (i, j + 1));
                                    }
                                    else if (_cubesInfo[i - 1, j].IsFree)
                                    {
                                        WrapCubes(_cubesInfo[i - 1, j], (i, j), (i - 1, j));
                                    }
                                }
                                else if (j == 3)
                                {
                                    if (_cubesInfo[i, j - 1].IsFree)
                                    {
                                        WrapCubes(_cubesInfo[i, j - 1], (i, j), (i, j - 1));
                                    }
                                    else if (_cubesInfo[i - 1, j].IsFree)
                                    {
                                        WrapCubes(_cubesInfo[i - 1, j], (i, j), (i - 1, j));
                                    }
                                }
                                else
                                {
                                    if (_cubesInfo[i - 1, j].IsFree)
                                    {
                                        WrapCubes(_cubesInfo[i - 1, j], (i, j), (i - 1, j));
                                    }
                                    else if (_cubesInfo[i, j + 1].IsFree)
                                    {
                                        WrapCubes(_cubesInfo[i, j + 1], (i, j), (i, j + 1));
                                    }
                                    else if (_cubesInfo[i, j - 1].IsFree)
                                    {
                                        WrapCubes(_cubesInfo[i, j - 1], (i, j), (i, j - 1));
                                    }
                                }
                            }
                            else if (j == 0)
                            {
                                if (i == 0)
                                {
                                    if (_cubesInfo[i, j + 1].IsFree)
                                    {
                                        WrapCubes(_cubesInfo[i, j + 1], (i, j), (i, j + 1));
                                    }
                                    else if (_cubesInfo[i + 1, j].IsFree)
                                    {
                                        WrapCubes(_cubesInfo[i + 1, j], (i, j), (i + 1, j));
                                    }
                                }
                                else if (i == 3)
                                {
                                    if (_cubesInfo[i, j + 1].IsFree)
                                    {
                                        WrapCubes(_cubesInfo[i, j + 1], (i, j), (i, j + 1));
                                    }
                                    else if (_cubesInfo[i - 1, j].IsFree)
                                    {
                                        WrapCubes(_cubesInfo[i - 1, j], (i, j), (i - 1, j));
                                    }
                                }
                                else
                                {
                                    if (_cubesInfo[i, j + 1].IsFree)
                                    {
                                        WrapCubes(_cubesInfo[i, j + 1], (i, j), (i, j + 1));
                                    }
                                    else if (_cubesInfo[i + 1, j].IsFree)
                                    {
                                        WrapCubes(_cubesInfo[i + 1, j], (i, j), (i + 1, j));
                                    }
                                    else if (_cubesInfo[i - 1, j].IsFree)
                                    {
                                        WrapCubes(_cubesInfo[i - 1, j], (i, j), (i - 1, j));
                                    }
                                }
                            }
                            else if (j == 3)
                            {
                                if (i == 0)
                                {
                                    if (_cubesInfo[i, j - 1].IsFree)
                                    {
                                        WrapCubes(_cubesInfo[i, j - 1], (i, j), (i, j - 1));
                                    }
                                    else if (_cubesInfo[i + 1, j].IsFree)
                                    {
                                        WrapCubes(_cubesInfo[i + 1, j], (i, j), (i + 1, j));
                                    }
                                }
                                else if (i == 3)
                                {
                                    if (_cubesInfo[i, j - 1].IsFree)
                                    {
                                        WrapCubes(_cubesInfo[i, j - 1], (i, j), (i, j - 1));
                                    }
                                    else if (_cubesInfo[i - 1, j].IsFree)
                                    {
                                        WrapCubes(_cubesInfo[i - 1, j], (i, j), (i - 1, j));
                                    }
                                }
                                else
                                {
                                    if (_cubesInfo[i - 1, j].IsFree)
                                    {
                                        WrapCubes(_cubesInfo[i - 1, j], (i, j), (i - 1, j));
                                    }
                                    else if (_cubesInfo[i + 1, j].IsFree)
                                    {
                                        WrapCubes(_cubesInfo[i + 1, j], (i, j), (i + 1, j));
                                    }
                                    else if (_cubesInfo[i, j - 1].IsFree)
                                    {
                                        WrapCubes(_cubesInfo[i, j - 1], (i, j), (i, j - 1));
                                    }
                                }
                            }
                            else
                            {
                                if (_cubesInfo[i - 1, j].IsFree)
                                {
                                    WrapCubes(_cubesInfo[i - 1, j], (i, j), (i - 1, j));
                                }
                                else if (_cubesInfo[i + 1, j].IsFree)
                                {
                                    WrapCubes(_cubesInfo[i + 1, j], (i, j), (i + 1, j));
                                }
                                else if (_cubesInfo[i, j - 1].IsFree)
                                {
                                    WrapCubes(_cubesInfo[i, j - 1], (i, j), (i, j - 1));
                                }
                                else if (_cubesInfo[i, j + 1].IsFree)
                                {
                                    WrapCubes(_cubesInfo[i, j + 1], (i, j), (i, j + 1));
                                }
                            }
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async void WrapCubes(CubeInfo to, (int, int) from, (int, int) toCoords)
        {
            _cubesInfo[toCoords.Item1, toCoords.Item2] = _cubesInfo[from.Item1, from.Item2];
            _cubesInfo[from.Item1, from.Item2] = new CubeInfo();

            Button newCube = new Button();
            newCube.BackgroundColor = Color.Transparent;

            Button tempCube = _playGrid[_COLUMNS_AMOUNT * from.Item1 + from.Item2];

            _playGrid.RemoveAt(_COLUMNS_AMOUNT * from.Item1 + from.Item2);
            _playGrid.Insert(_COLUMNS_AMOUNT * from.Item1 + from.Item2, newCube);
            _playGrid.RemoveAt(_COLUMNS_AMOUNT * toCoords.Item1 + toCoords.Item2);
            _playGrid.Insert(_COLUMNS_AMOUNT * toCoords.Item1 + toCoords.Item2, tempCube);

            RefreshCubes();

            int previousScore = int.Parse(CurrentScore);
            CurrentScore = (++previousScore).ToString();

            if (IsWin())
            {
                await SaveResult();
                ResetData();
                await Shell.Current.GoToAsync("//ResultPage");
            }
        }

        private async void OnGameOverClicked(object obj)
        {
            ResetData();
            await Shell.Current.GoToAsync("//FirstGameMenuPage");
        }

        private bool IsWin()
        {
            try
            {
                int counter = 0;
                for (int i = 0; i < _ROWS_AMOUNT; i++)
                {
                    for (int j = 0; j < _COLUMNS_AMOUNT; j++, counter++)
                    {
                        if (_playGrid[counter].Text is null)
                        {
                            continue;
                        }
                        if (!_playGrid[counter].Text.Equals((counter + 1).ToString()))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                return false;
            }
        }

        private void ResetData()
        {
            _cubesInfo = new CubeInfo[_ROWS_AMOUNT, _COLUMNS_AMOUNT];
            _playField.Children.Clear();
            _playGrid.Clear();
            MainDataStore.PreviousScore = CurrentScore;
            CurrentScore = "0";
            IsInitialized = false;

        }

        private async Task SaveResult()
        {
            string dbPath = DependencyService.Get<IPath>().GetDatabasePath(App.DBFILENAME);
            using (var context = new ApplicationContext(dbPath))
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.Username.Equals(MainDataStore.Username));
                if (user is null)
                {
                    throw new NullReferenceException("User is not found!");
                }

                Result newResult = new Result();
                newResult.Steps = int.Parse(CurrentScore);
                newResult.UserId = user.Id;

                context.Results.Add(newResult);
                await context.SaveChangesAsync();
            }
        }
    }
}
