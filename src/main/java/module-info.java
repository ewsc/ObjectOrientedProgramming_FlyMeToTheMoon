module com.alamov.flymetothemoon {
    requires javafx.controls;
    requires javafx.fxml;

    requires org.controlsfx.controls;

    opens com.alamov.flymetothemoon to javafx.fxml;
    exports com.alamov.flymetothemoon;
}