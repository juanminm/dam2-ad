﻿using Gtk;
using MySql.Data.MySqlClient;
using System;
using System.Data;

using CCategoria;

public partial class MainWindow : Gtk.Window
{
    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {
        Build();
        Title = "Categoria";

        deleteAction.Sensitive = false;

        App.Instance.Connection = new MySqlConnection("server=localhost;"
                + "database=dbprueba;user=root;password=sistemas");
        App.Instance.Connection.Open();

        treeView.AppendColumn("id", new CellRendererText(), "text", 0);
        treeView.AppendColumn("nombre", new CellRendererText(), "text", 1);
        ListStore listStore = new ListStore(typeof(string), typeof(string));
        treeView.Model = listStore;

        FillListStore(listStore);

        newAction.Activated += delegate {
            new CategoriaView();
        };

        refreshAction.Activated += delegate {
            listStore.Clear();

            FillListStore(listStore);
        };

        treeView.Selection.Changed += delegate {
            deleteAction.Sensitive = treeView.Selection.CountSelectedRows() > 0;
        };

        deleteAction.Activated += delegate {
            if (WindowHelper.Confirm(this, "¿Quieres eliminar el registro?"))
            {
                IDbCommand dbCommand = App.Instance.Connection.CreateCommand();

                dbCommand.CommandText = "DELETE FROM `categoria` (`nombre`) " +
                    "WHERE `id` = @id";
                DbCommandHelper.AddParameter(dbCommand, "id", GetId());
                dbCommand.ExecuteNonQuery();
            }
        };
    }

    private object GetId()
    {
        treeView.Selection.GetSelected(out TreeIter treeIter);
        return treeView.Model.GetValue(treeIter, 0);
    }

    private void FillListStore(ListStore listStore)
    {
        IDbCommand dbCommand = App.Instance.Connection.CreateCommand();
        IDataReader dataReader;
        dbCommand.CommandText = "SELECT * FROM `categoria` ORDER BY `id`";
        dataReader = dbCommand.ExecuteReader();

        while (dataReader.Read())
        {
            listStore.AppendValues(dataReader["id"].ToString(),
                                   dataReader["nombre"].ToString());
        }
        dataReader.Close();
    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        App.Instance.Connection.Close();
        Application.Quit();
        a.RetVal = true;
    }
}
