#include "stdio.h"
#include "BMP280_driver/bmp280.h"
#include <fcntl.h>
#include <unistd.h>
#include <sys/ioctl.h>
#include <linux/i2c-dev.h>
#include <errno.h>

void delay_ms(uint32_t period_ms);
int8_t i2c_reg_write(uint8_t i2c_addr, uint8_t reg_addr, uint8_t* reg_data, uint16_t length);
int8_t i2c_reg_read(uint8_t i2c_addr, uint8_t reg_addr, uint8_t* reg_data, uint16_t length);
void print_rslt(const char api_name[], int8_t rslt);

static struct bmp280_dev bmp;
static struct bmp280_config conf;

void bmp280lib_init() {
	printf("init");
    /* Map the delay function pointer with the function responsible for implementing the delay */
    bmp.delay_ms = delay_ms;

    /* Assign device I2C address based on the status of SDO pin (GND for PRIMARY(0x76) & VDD for SECONDARY(0x77)) */
    bmp.dev_id = BMP280_I2C_ADDR_SEC;

    /* Select the interface mode as I2C */
    bmp.intf = BMP280_I2C_INTF;

    /* Map the I2C read & write function pointer with the functions responsible for I2C bus transfer */
    bmp.read = i2c_reg_read;
    bmp.write = i2c_reg_write;

    int8_t rslt = bmp280_init(&bmp);
    print_rslt(" bmp280_init status", rslt);

    /* Always read the current settings before writing, especially when
     * all the configuration is not modified
     */
    rslt = bmp280_get_config(&conf, &bmp);
    print_rslt(" bmp280_get_config status", rslt);

    /* configuring the temperature oversampling, filter coefficient and output data rate */
    /* Overwrite the desired settings */
    conf.filter = BMP280_FILTER_COEFF_4;

    /* Temperature oversampling set at 1x */
    conf.os_temp = BMP280_OS_2X;

    /* Pressure oversampling set at 8x */
    conf.os_pres = BMP280_OS_16X;

    /* Setting the output data rate as 1HZ(1000ms) */
    conf.odr = BMP280_ODR_500_MS;
    rslt = bmp280_set_config(&conf, &bmp);
    print_rslt(" bmp280_set_config status", rslt);

    /* Always set the power mode after setting the configuration */
    rslt = bmp280_set_power_mode(BMP280_NORMAL_MODE, &bmp);
    print_rslt(" bmp280_set_power_mode status", rslt);
	// Wait for some messurements
	//delay_ms(1000);
}

double bmp280lib_get_temp() {
    struct bmp280_uncomp_data ucomp_data;
	struct bmp280_status status;
    int32_t temp32;
    double temp;
	int8_t rslt = bmp280_set_power_mode(BMP280_FORCED_MODE, &bmp);
    print_rslt(" bmp280_set_power_mode status", rslt);
	delay_ms(600);
	bmp280_get_status(&status, &bmp);
	printf("status: %d %d", status.measuring, status.im_update);
    /* Reading the raw data from sensor */
    rslt = bmp280_get_uncomp_data(&ucomp_data, &bmp);
	print_rslt(" bmp280_get_uncomp_data", rslt);
    /* Getting the 32 bit compensated temperature */
    rslt = bmp280_get_comp_temp_32bit(&temp32, ucomp_data.uncomp_temp, &bmp);
	print_rslt(" bmp280_get_comp_temp_32bit", rslt);
    /* Getting the compensated temperature as floating point value */
    rslt = bmp280_get_comp_temp_double(&temp, ucomp_data.uncomp_temp, &bmp);
	print_rslt(" bmp280_get_comp_temp_double", rslt);
	printf("UT: %ld, T32: %ld, T: %f \r\n", ucomp_data.uncomp_temp, temp32, temp);
	return temp;
}

double bmp280lib_get_press() {
    struct bmp280_uncomp_data ucomp_data;
    uint32_t pres32, pres64;
    double pres;

    /* Reading the raw data from sensor */
    int8_t rslt = bmp280_get_uncomp_data(&ucomp_data, &bmp);
	print_rslt(" bmp280_get_uncomp_data", rslt);
    /* Getting the compensated pressure using 32 bit precision */
    rslt = bmp280_get_comp_pres_32bit(&pres32, ucomp_data.uncomp_press, &bmp);
	print_rslt(" bmp280_get_comp_pres_32bit", rslt);
    /* Getting the compensated pressure using 64 bit precision */
    rslt = bmp280_get_comp_pres_64bit(&pres64, ucomp_data.uncomp_press, &bmp);
	print_rslt(" bmp280_get_comp_pres_64bit", rslt);
    /* Getting the compensated pressure as floating point value */
    rslt = bmp280_get_comp_pres_double(&pres, ucomp_data.uncomp_press, &bmp);
	print_rslt(" bmp280_get_comp_pres_double", rslt);
    return pres;
}

/*!
 *  @brief Function that creates a mandatory delay required in some of the APIs such as "bmg250_soft_reset",
 *      "bmg250_set_foc", "bmg250_perform_self_test"  and so on.
 *
 *  @param[in] period_ms  : the required wait time in milliseconds.
 *  @return void.
 *
 */
void delay_ms(uint32_t period_ms)
{
    /* Implement the delay routine according to the target machine */
    usleep(period_ms);
}

/*!
 *  @brief Function for writing the sensor's registers through I2C bus.
 *
 *  @param[in] i2c_addr : sensor I2C address.
 *  @param[in] reg_addr : Register address.
 *  @param[in] reg_data : Pointer to the data buffer whose value is to be written.
 *  @param[in] length   : No of bytes to write.
 *
 *  @return Status of execution
 *  @retval 0 -> Success
 *  @retval >0 -> Failure Info
 *
 */
int8_t i2c_reg_write(uint8_t i2c_addr, uint8_t reg_addr, uint8_t* reg_data, uint16_t length)
{
    /* init i2c */
    int fd;
    char* filename = (char*)"/dev/i2c-1";
    if ((fd = open(filename, O_RDWR)) < 0)
    {
        printf("Open failed errno: %d\n", errno);
        return -1;
    }

    if (ioctl(fd, I2C_SLAVE, i2c_addr) < 0)
    {
        printf("ioctl failed errno: %d\n", errno);
        return -1;
    }
	// Write reg address
	if(write(fd, &reg_addr, 1) != 1) {
		printf("could not write reg addr errno %d\n");
		return -1;
	}
    /* Implement the I2C write routine according to the target machine. */
    if (write(fd, reg_data, length) != length) {
        printf("write failed errno %d\n", errno);
        return -1;
    }
    if(close(fd) < 0) {
		printf("close failed errno %d\n", errno);
		return -1;
	}
    return 0;
}

/*!
 *  @brief Function for reading the sensor's registers through I2C bus.
 *
 *  @param[in] i2c_addr : Sensor I2C address.
 *  @param[in] reg_addr : Register address.
 *  @param[out] reg_data    : Pointer to the data buffer to store the read data.
 *  @param[in] length   : No of bytes to read.
 *
 *  @return Status of execution
 *  @retval 0 -> Success
 *  @retval >0 -> Failure Info
 *
 */
int8_t i2c_reg_read(uint8_t i2c_addr, uint8_t reg_addr, uint8_t* reg_data, uint16_t length)
{

    /* Implement the I2C read routine according to the target machine. */
    int fd;
    char* filename = (char*)"/dev/i2c-1";
    if ((fd = open(filename, O_RDWR)) < 0)
    {
        printf("Open failed errno: %d\n", errno);
        return -1;
    }

    if (ioctl(fd, I2C_SLAVE, i2c_addr) < 0)
    {
        printf("ioctl failed errno: %d\n", errno);
        return -1;
    }
		// Write reg address
	if(write(fd, &reg_addr, 1) != 1) {
		printf("could not write reg addr errno %d\n");
		return -1;
	}
    if (read(fd, reg_data, length) != length) {
        printf("read failed. errno %d\n", errno);
        return -1;
    }
	if(close(fd) < 0) {
		printf("close failed errno %d\n", errno);
		return -1;
	}
    return 0;
}

/*!
 *  @brief Prints the execution status of the APIs.
 *
 *  @param[in] api_name : name of the API whose execution status has to be printed.
 *  @param[in] rslt     : error code returned by the API whose execution status has to be printed.
 *
 *  @return void.
 */
void print_rslt(const char api_name[], int8_t rslt)
{
    if (rslt != BMP280_OK)
    {
        printf("%s\t", api_name);
        if (rslt == BMP280_E_NULL_PTR)
        {
            printf("Error [%d] : Null pointer error\r\n", rslt);
        }
        else if (rslt == BMP280_E_COMM_FAIL)
        {
            printf("Error [%d] : Bus communication failed\r\n", rslt);
        }
        else if (rslt == BMP280_E_IMPLAUS_TEMP)
        {
            printf("Error [%d] : Invalid Temperature\r\n", rslt);
        }
        else if (rslt == BMP280_E_DEV_NOT_FOUND)
        {
            printf("Error [%d] : Device not found\r\n", rslt);
        }
        else
        {
            /* For more error codes refer "*_defs.h" */
            printf("Error [%d] : Unknown error code\r\n", rslt);
        }
    }
}
