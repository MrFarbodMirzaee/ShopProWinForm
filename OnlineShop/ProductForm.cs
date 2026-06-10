using ShopPro.BaseBackend.Models;
using ShopPro.BaseBackend.Repositories;
using System.Data.SqlClient;



namespace ShopPro;

public partial class ProductForm : Form
{
    byte[] imageData;
    Product selectedproduct;

    public delegate void RefreshDataDelegate();
    public event RefreshDataDelegate RefreshDataEvent;
    public ProductForm()
    {
        InitializeComponent();
        ProductRepository repository = new ProductRepository();
        RefreshDataEvent += RefreshDate; //Delegate
        LoadFromData();
    }
    private void RefreshDate()
    {
        ProductRepository repository = new ProductRepository();
        List<Product> products = repository.GetAll();
        ProductDataGridView.DataSource = null;
        ProductDataGridView.DataSource = products;
        ProductDataGridView.Refresh();

    }
    public void LoadFromData()
    {
        RefreshDate();
    }
    private void ProductForm_Load(object sender, EventArgs e)
    {

    }
    private bool ValidateProductData()
    {
        if (string.IsNullOrEmpty(NameTextBox.Text) ||
            string.IsNullOrEmpty(BrandTextBox.Text) ||
            string.IsNullOrEmpty(PriceTextBox.Text) ||
            string.IsNullOrEmpty(SpecificationTextBox.Text) ||
            string.IsNullOrEmpty(CountTextBox.Text))
        {
         
            MessageBox.Show("Error: Please fill in all required product fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false; 
        }
        if (NameTextBox.Text.Length < 3 || BrandTextBox.Text.Length < 3 ) 
        {
            MessageBox.Show("Error: Price must be a valid decimal value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
            decimal price;
        if (!decimal.TryParse(PriceTextBox.Text, out price))
        {
          
            MessageBox.Show("Error: Price must be a valid decimal value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false; 
        }

        short count;
        if (!short.TryParse(CountTextBox.Text, out count))
        {
           
            MessageBox.Show("Error: Count must be a valid numeric value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false; 
        }

        if (imageData == null)
        {
       
            MessageBox.Show("Error: Please import a picture in the picture box.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false; 
        }

        return true;
    }
    private void SaveButton_Click(object sender, EventArgs e)
    {

        if (ValidateProductData()) 
        {
            Product product = new Product()
            {
                Name = NameTextBox.Text,
                Brand = BrandTextBox.Text,
                Price = Convert.ToDecimal(PriceTextBox.Text),
                Image = imageData,
                Specificaion = SpecificationTextBox.Text,
                Count = short.Parse(CountTextBox.Text),
                IsActive = IsActiveRadioButton.Checked,
            };
            ProductRepository repository = new ProductRepository();
            repository.Insert(product);
            RefreshDataEvent?.Invoke();
            Clear();
        }
    }
    private void ProductPictureBox_Click(object sender, EventArgs e)
    {
        OpenFileDialog openFileDialog1 = new OpenFileDialog();

      
        openFileDialog1.Filter = "Image Files|*.jpg;*.jpeg;*.png;";

        if (openFileDialog1.ShowDialog() == DialogResult.OK)
        {
          
            ProductPictureBox.Image = new Bitmap(openFileDialog1.FileName);

            using (MemoryStream ms = new MemoryStream())
            {
                ProductPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                ProductPictureBox.Image.Save(ms, ProductPictureBox.Image.RawFormat);
                imageData = ms.ToArray();
            }
        }
    }

    private void ResetButton_Click(object sender, EventArgs e)
    {
        Clear();
    }
    private void ProductDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
    {
        if (ProductDataGridView.SelectedCells.Count > 0)
        {
            var selectedindex = ProductDataGridView.SelectedCells[0].RowIndex;
            var row = ProductDataGridView.Rows[selectedindex];
            int id = int.Parse(row.Cells["Id"].Value.ToString());
            ProductRepository repository = new ProductRepository();
            Product product = repository.GetById(id);
            NameTextBox.Text = product.Name;
            PriceTextBox.Text = product.Price.ToString();
            SpecificationTextBox.Text = product.Specificaion;

            using (MemoryStream ms = new MemoryStream(product.Image))
            {
                ProductPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                ProductPictureBox.Image = Image.FromStream(ms);
            }

            BrandTextBox.Text = product.Brand;
            CountTextBox.Text = product.Count.ToString();
            selectedproduct = product;
        }
    }
    private void UpdateButton_Click(object sender, EventArgs e)
    {
        if (selectedproduct is null)
        {
            MessageBox.Show("Please select an item in data grid view");
            return;
        }
        ProductRepository repository = new ProductRepository();
        selectedproduct.Name = NameTextBox.Text;
        selectedproduct.Specificaion = SpecificationTextBox.Text;
        selectedproduct.Price = decimal.Parse(PriceTextBox.Text);
        selectedproduct.Brand = BrandTextBox.Text;
        selectedproduct.Count = short.Parse(CountTextBox.Text);
        try
        {
            using (MemoryStream ms = new MemoryStream())
            {
                if (ProductPictureBox.Image != null)
                {
                    // Create a copy of the image using a Bitmap object
                    Bitmap bitmap = new Bitmap(ProductPictureBox.Image);
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    selectedproduct.Image = ms.ToArray();
                    bitmap.Dispose();  // Dispose of the temporary Bitmap object
                }
                else
                {
                    MessageBox.Show("The ProductPictureBox does not contain a valid image.");
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("An error occurred while processing the image: " + ex.Message);
        }
        selectedproduct.IsActive = IsActiveRadioButton.Checked;
        repository.Update(selectedproduct);
        RefreshDataEvent?.Invoke();
        Clear();

    }
    private void DeleteButton_Click(object sender, EventArgs e)
    {
        if (selectedproduct is null)
        {
            MessageBox.Show("Please select an item in the product data grid view", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        ProductRepository repository = new ProductRepository();
        try
        {
            repository.Delete(id: selectedproduct.Id);
            RefreshDataEvent?.Invoke();
            Clear();
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            Console.WriteLine("Foreign key constraint violation occurred.");
            MessageBox.Show("Sorry, We cannot delete this record as it may be referenced by OrderForm.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Clear()
    {
        NameTextBox.Text = string.Empty;
        CountTextBox.Text = string.Empty;
        PriceTextBox.Text = string.Empty;
        BrandTextBox.Text = string.Empty;
        SpecificationTextBox.Text = string.Empty;
        IsActiveRadioButton.Checked = false;
        ProductPictureBox.Image.Dispose();
    }

    private void NameTextBox_KeyPress(object sender, KeyPressEventArgs e)
    {
       
         if (NameTextBox.Text.Length <= 2)
        {
            ErrorNamelabel.Text = "Error: Name must be at least two characters long"; 
            ErrorNamelabel.ForeColor = Color.Red;
            ErrorNamelabel.Visible = true; 
        }
        else if (NameTextBox.Text.Length > 300 && e.KeyChar != (char)Keys.Back)
        {
            e.Handled = true; 
            ErrorNamelabel.Text = "Error: Name cannot exceed 300 characters";
            ErrorNamelabel.ForeColor = Color.Red;
            ErrorNamelabel.Visible = true;
        }
        else
        {
            ErrorNamelabel.Visible = false;
        }
    }

    private void PriceTextBox_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
        {
            ErrorPricelabel.ForeColor = Color.Red;
            ErrorPricelabel.Text = "You can't enter words in Price text box";
            ErrorPricelabel.Visible = true;
            e.Handled = true;
        }
        else if (PriceTextBox.Text.Length >= 18 && !char.IsControl(e.KeyChar))
        {
            ErrorPricelabel.ForeColor = Color.Red;
            ErrorPricelabel.Text = "Price must be 18 character";
            ErrorPricelabel.Visible = true;
            e.Handled = true;
        }
        else
        {
            e.Handled = false;
            ErrorPricelabel.Visible = false;
        }
    }

    private void BrandTextBox_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (char.IsDigit(e.KeyChar)) 
        {
            e.Handled = true;
            ErrorBrandLabel.Text = "Error: Numbers are not allowed in the Brand"; 
            ErrorBrandLabel.ForeColor = Color.Red; 
            ErrorBrandLabel.Visible = true; 
        }
        else if (BrandTextBox.Text.Length <= 2)
        {
            ErrorBrandLabel.Text = "Error: Brand must be at least two characters long"; 
            ErrorBrandLabel.ForeColor = Color.Red; 
            ErrorBrandLabel.Visible = true; 
        }
        else if (BrandTextBox.Text.Length > 300 && e.KeyChar != (char)Keys.Back)
        {
            e.Handled = true; 
            ErrorBrandLabel.Text = "Error: Brand cannot exceed 300 characters";
            ErrorBrandLabel.ForeColor = Color.Red; 
            ErrorBrandLabel.Visible = true; 
        }
        else
        {
            ErrorBrandLabel.Visible = false;
        }
    }

    private void CountTextBox_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
        {
            ErrorCountlabel.ForeColor = Color.Red;
            ErrorCountlabel.Text = "You can't enter words in Count text box";
            ErrorCountlabel.Visible = true;
            e.Handled = true;
        }
        else if (CountTextBox.Text.Length >= 3 && !char.IsControl(e.KeyChar))
        {
            ErrorCountlabel.ForeColor = Color.Red;
            ErrorCountlabel.Text = "Count can not be more 300 character";
            ErrorCountlabel.Visible = true;
            e.Handled = true;
        }
        else
        {
            e.Handled = false;
            ErrorCountlabel.Visible = false;
        }
    }
    private void homeToolStripMenuItem_Click(object sender, EventArgs e)
    {
        HomeForm homeForm = new HomeForm();
        homeForm.Show();
        this.Close();
    }

    private void customerToolStripMenuItem_Click(object sender, EventArgs e)
    {
        CustomerForm customerForm = new CustomerForm();
        customerForm.Show();
        this.Close();
    }

    private void orderToolStripMenuItem_Click(object sender, EventArgs e)
    {
        OrderForm orderForm = new OrderForm();
        orderForm.Show();
        this.Close();
    }
}
