using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Sigebi.Desktop.Models;
using Sigebi.Desktop.Services;

namespace Sigebi.Desktop;

public partial class MainWindow : Window
{
    private readonly SigebiApiService _api = new();
    private bool _isBusy;

    private void SetBusy(bool busy)
    {
        _isBusy = busy;
        BusyOverlay.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        MainTabs.IsEnabled = !busy;
    }

    public MainWindow()
    {
        InitializeComponent();
        Loaded += async (_, _) => await SafeRunAsync(InitialLoadAsync);
    }

    private async Task InitialLoadAsync()
    {
        SetBusy(true);
        await RefreshBooksGridAsync();
        await RefreshPendingAsync();
        await RefreshActiveLoansAsync();
        await RefreshReportsAsync();
        SetBusy(false);
    }

    private async void OnSearchBooksClick(object sender, RoutedEventArgs e) =>
        await SafeRunAsync(RefreshBooksGridAsync);

    private async Task RefreshBooksGridAsync()
    {
        var query = InventorySearchTab?.Text ?? string.Empty;
        var books = await _api.SearchBooksAsync(query).ConfigureAwait(true);
        var rows = new List<object>();
        foreach (var b in books)
        {
            foreach (var c in b.Copies)
            {
                rows.Add(new
                {
                    b.BookId,
                    b.Title,
                    b.Author,
                    b.Isbn,
                    b.Category,
                    c.CopyId,
                    c.InventoryCode,
                    c.Status
                });
            }
        }

        BooksGrid.ItemsSource = rows;
        SetStatus($"Catálogo: {rows.Count} filas de ejemplares.");
    }

    private async void OnRegisterBookClick(object sender, RoutedEventArgs e)
    {
        await SafeRunAsync(async () =>
        {
            await _api.RegisterBookAsync(
                NewTitle.Text,
                NewAuthor.Text,
                NewIsbn.Text,
                NewCategory.Text,
                NewInventory.Text).ConfigureAwait(true);
            SetStatus("Libro registrado.");
            await RefreshBooksGridAsync().ConfigureAwait(true);
        });
    }

    private async void OnDeleteBookClick(object sender, RoutedEventArgs e)
    {
        await SafeRunAsync(async () =>
        {
            if (!int.TryParse(DeleteBookId.Text, out var id))
            {
                SetStatus("Id de libro no válido.");
                return;
            }

            await _api.DeleteBookAsync(id).ConfigureAwait(true);
            SetStatus("Libro eliminado.");
            await RefreshBooksGridAsync().ConfigureAwait(true);
        });
    }

    private async void OnRefreshPendingClick(object sender, RoutedEventArgs e) =>
        await SafeRunAsync(RefreshPendingAsync);

    private async Task RefreshPendingAsync()
    {
        SetBusy(true);
        var list = await _api.GetPendingRequestsAsync().ConfigureAwait(true);
        PendingGrid.ItemsSource = list;
        SetStatus($"Solicitudes pendientes: {list.Count}.");
        SetBusy(false);
    }

    private async void OnApproveClick(object sender, RoutedEventArgs e)
    {
        await SafeRunAsync(async () =>
        {
            if (PendingGrid.SelectedItem is not LoanRequestModel row)
            {
                SetStatus("Selecciona una solicitud.");
                return;
            }

            await _api.ApproveRequestAsync(row.Id).ConfigureAwait(true);
            SetStatus("Solicitud aprobada.");
            await RefreshPendingAsync().ConfigureAwait(true);
            await RefreshActiveLoansAsync().ConfigureAwait(true);
            await RefreshBooksGridAsync().ConfigureAwait(true);
        });
    }

    private async void OnRejectClick(object sender, RoutedEventArgs e)
    {
        await SafeRunAsync(async () =>
        {
            if (PendingGrid.SelectedItem is not LoanRequestModel row)
            {
                SetStatus("Selecciona una solicitud.");
                return;
            }

            await _api.RejectRequestAsync(row.Id, RejectReason.Text).ConfigureAwait(true);
            SetStatus("Solicitud rechazada.");
            await RefreshPendingAsync().ConfigureAwait(true);
        });
    }

    private async void OnRefreshActiveClick(object sender, RoutedEventArgs e) =>
        await SafeRunAsync(RefreshActiveLoansAsync);

    private async Task RefreshActiveLoansAsync()
    {
        SetBusy(true);
        var list = await _api.GetActiveLoansAsync().ConfigureAwait(true);
        ActiveLoansGrid.ItemsSource = list;
        SetStatus($"Préstamos activos: {list.Count}.");
        SetBusy(false);
    }

    private async void OnReturnClick(object sender, RoutedEventArgs e)
    {
        await SafeRunAsync(async () =>
        {
            if (!int.TryParse(ReturnLoanId.Text, out var loanId))
            {
                SetStatus("Id de préstamo no válido.");
                return;
            }

            await _api.ReturnLoanAsync(loanId).ConfigureAwait(true);
            SetStatus("Devolución registrada.");
            await RefreshActiveLoansAsync().ConfigureAwait(true);
            await RefreshBooksGridAsync().ConfigureAwait(true);
            await RefreshReportsAsync().ConfigureAwait(true);
        });
    }

    private async void OnDirectLoanClick(object sender, RoutedEventArgs e)
    {
        await SafeRunAsync(async () =>
        {
            if (!int.TryParse(DirectUserId.Text, out var userId) || !int.TryParse(DirectCopyId.Text, out var copyId))
            {
                SetStatus("Usuario o ejemplar no válidos.");
                return;
            }

            await _api.DirectLoanAsync(userId, copyId).ConfigureAwait(true);
            SetStatus("Préstamo directo registrado.");
            await RefreshActiveLoansAsync().ConfigureAwait(true);
            await RefreshBooksGridAsync().ConfigureAwait(true);
        });
    }

    private async Task RefreshReportsAsync()
    {
        SetBusy(true);
        var overdue = await _api.GetOverdueAsync().ConfigureAwait(true);
        var penalties = await _api.GetPenaltiesAsync().ConfigureAwait(true);
        OverdueGrid.ItemsSource = overdue;
        PenaltiesGrid.ItemsSource = penalties.Where(p => !p.IsResolved).ToList();
        SetStatus($"Reportes: {overdue.Count} vencidos, {penalties.Count(p => !p.IsResolved)} sanciones abiertas.");
        SetBusy(false);
    }

    private async void OnResolvePenaltyClick(object sender, RoutedEventArgs e)
    {
        await SafeRunAsync(async () =>
        {
            if (!int.TryParse(PenaltyIdBox.Text, out var penaltyId))
            {
                SetStatus("Id de penalización no válido.");
                return;
            }

            await _api.ResolvePenaltyAsync(penaltyId).ConfigureAwait(true);
            SetStatus("Penalización resuelta.");
            await RefreshReportsAsync().ConfigureAwait(true);
        });
    }

    private void SetStatus(string message) => StatusText.Text = message;

    private async Task SafeRunAsync(Func<Task> action)
    {
        try
        {
            SetBusy(true);
            await action().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message);
            MessageBox.Show(this, ex.Message, "SIGEBI", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void OnActiveLoansGridDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (ActiveLoansGrid.SelectedItem is ActiveLoanModel loan)
        {
            MessageBox.Show(this, $"Préstamo Id: {loan.LoanId}\nUsuario Id: {loan.UserId}\nCódigo inventario: {loan.InventoryCode}\nVence: {loan.DueDate:d}", "Detalle préstamo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            SetStatus("Selecciona un préstamo para ver detalles.");
        }
    }

    private async void OnInventorySearchKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter)
            await SafeRunAsync(RefreshBooksGridAsync);
    }

    private void OnThemeChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        // theme selector removed from UI
    }

    private void RejectReason_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {

    }
}
