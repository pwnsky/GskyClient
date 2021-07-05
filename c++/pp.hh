#pragma once
/*
 * Pwnsky Protocl C++ Client SDK
*/

#include <iostream>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <cstring>
#include <memory>
#include <unistd.h>
#include <string>
#include <malloc.h>

/*
 * Define pwnsky protocol
 * */
namespace pp {

#define VESSEL_DEFAULT_SIZE  0x18
#define VESSEL_DEFAULT_ALIGN 0x20

class vessel {
public:
    vessel() :
        size_(0),
        capacity_(VESSEL_DEFAULT_SIZE),
        data_ptr_(static_cast<char*>(malloc(VESSEL_DEFAULT_SIZE))) {
        data_offset_ = 0;
    }
    ~vessel() {
        free(data_ptr_);
    };

    //调整内存容量
    void resize(size_t size) {
        size_t mem_size = size + data_offset_;
        void* ret_ptr = realloc(data_ptr_, mem_size);
        if(ret_ptr == nullptr) {
            std::cout << "data realloc, can't resize\n";
            return;
        }
        if(size_ > size) {
            size_ = size;
        }
        data_ptr_ = static_cast<char *>(ret_ptr);
        capacity_ = mem_size;
    }

    void operator<<(std::string data) {
        size_t size =  data.size();
        if((capacity_ - data_offset_ - size_) < size) {
           size_t mem_size = align(capacity_ + size);
           void* ret_ptr = realloc(data_ptr_, mem_size);
           if(ret_ptr == nullptr) {
               std::cout << "data realloc, can't append\n";
               return;
           }
            data_ptr_ = static_cast<char *>(ret_ptr);
            capacity_ = mem_size;
            times_ ++;
        }
        memcpy(data_ptr_ + data_offset_ + size_, data.data(), size); //拷贝至数据末尾
        size_ += size;
    }

    void operator<<(const char *data) {
        size_t size =  strlen(data);
        if((capacity_ - data_offset_ - size_) < size) {
           size_t mem_size = align(capacity_ + size);
           void* ret_ptr = realloc(data_ptr_, mem_size);
           if(ret_ptr == nullptr) {
               std::cout << "data realloc, can't append\n";
               return;;
           }
            data_ptr_ = static_cast<char *>(ret_ptr);
            capacity_ = mem_size;
            times_ ++;
        }
        memcpy(data_ptr_ + size_, data, size);
        size_ += size;
    }
    void append(void *data, size_t length) {
        size_t size = length;
        if((capacity_ - data_offset_ - size_) < size) {
           size_t mem_size = align(capacity_ + size);
           void* ret_ptr = realloc(data_ptr_, mem_size);
           if(ret_ptr == nullptr) {
               std::cout << "data realloc, can't append\n";
               return;;
           }
            data_ptr_ = static_cast<char *>(ret_ptr);
            capacity_ = mem_size;
            times_ ++;
        }
        memcpy(data_ptr_ + data_offset_ + size_, data, size);
        size_ += size;
    }

    void sub(size_t size) {
        if(size_ < size) {
            std::cout << "sub data : out of range\n";
            size_ = 0;
            return ;
        }
        size_ -= size;
        data_offset_ += size;
    }

    int find(char c) {
        size_t i = 0;
        for(i = 0; i < size_; ++i)
            if(data_ptr_[i + data_offset_] == c)
                break;
        if(i == size_)
            return -1;
        return i;
    }

    int find(const char *ts) {
        const char *bp;
        const char *sp;
        const char *src = data_ptr_ + data_offset_;
        for(size_t i = 0; i < size_; ++i) {
            bp = src;
            sp = ts;
            do {
                if(!*sp)
                    return (int)(src - data_ptr_ - data_offset_);
            } while(*bp ++ == *sp ++);
            src ++;
        }
        return -1;
    }

    int find_to_end(const char *ts) {
        const char *bp;
        const char *sp;
        const char *src = data_ptr_ + data_offset_;
        for(size_t i = 0; i < size_; ++i) {
            bp = src;
            sp = ts;
            do {
                if(!*sp)
                    return (int)(bp - data_ptr_ - data_offset_);
            } while(*bp ++ == *sp ++);
            src ++;
        }
        return -1;
        return 0;
    }

    int find(const char *ts, int length) {
        const char *bp;
        const char *sp;
        const char *src = data_ptr_ + data_offset_;
        for(size_t i = 0; i < size_; ++i) {
            bp = src;
            sp = ts;
            int len = 0;
            do {
                if(len == length)
                    return (int)(src - data_ptr_ - data_offset_);
                len ++;
            } while(*bp ++ == *sp ++);
            src ++;
        }
        return -1;
    }

    std::string get_string(int start, int length) {
        return std::string(data_ptr_ + data_offset_ + start, length);
    }

    void clear() {
        size_ = 0;
        data_offset_ = 0;
        free(data_ptr_);
        capacity_ = VESSEL_DEFAULT_SIZE;
        data_ptr_ = (char *)malloc(VESSEL_DEFAULT_SIZE);
    }

    char *data() {
        return data_ptr_ + data_offset_;
    }

    std::string to_string() {
        return std::string(data_ptr_ + data_offset_, data_ptr_ + data_offset_ + size_);
    }

    size_t size() {
        return size_;
    }
    size_t capacity() {
        return capacity_;
    }

    size_t offset() {
        return data_offset_;
    }

private:
    size_t size_;
    size_t capacity_;
    size_t data_offset_;
    char *data_ptr_;
    char times_ = 1; // append 时开辟大小 2次方增加，进可能的避免多次 realloc。

    size_t align(size_t size) {
        size_t n = size / VESSEL_DEFAULT_ALIGN;
        if(size % VESSEL_DEFAULT_ALIGN == 0)
            return n * VESSEL_DEFAULT_ALIGN;
        return  ((n + 1) * VESSEL_DEFAULT_ALIGN) * times_ * times_;
    }
};

}

namespace pp {
class pe {
public:
    static void encode(unsigned char key[8], void *raw_data, size_t length);

    static void decode(unsigned char key[8], void *raw_data, size_t length);

    static unsigned char xor_table_[256];
private:
    static void key_random(unsigned char raw_key[8], unsigned char *out_key, unsigned char seed);
};

}

unsigned char pp::pe::xor_table_[256] = {
    0xbe, 0xd1, 0x90, 0x88, 0x57, 0x00, 0xe9, 0x53, 0x10, 0xbd, 0x2a, 0x34, 0x51, 0x84, 0x07, 0xc4,
    0x33, 0xc5, 0x3b, 0x53, 0x5f, 0xa8, 0x5d, 0x4b, 0x6d, 0x22, 0x63, 0x5d, 0x3c, 0xbd, 0x47, 0x6d,
    0x22, 0x3f, 0x38, 0x4b, 0x7a, 0x4c, 0xb8, 0xcc, 0xb8, 0x37, 0x78, 0x17, 0x73, 0x23, 0x27, 0x71,
    0xb1, 0xc7, 0xa6, 0xd1, 0xa0, 0x48, 0x21, 0xc4, 0x1b, 0x0a, 0xad, 0xc9, 0xa5, 0xe6, 0x14, 0x18,
    0xfc, 0x7b, 0x53, 0x59, 0x8b, 0x0d, 0x07, 0xcd, 0x07, 0xcc, 0xbc, 0xa5, 0xe0, 0x28, 0x0e, 0xf9,
    0x31, 0xc8, 0xed, 0x78, 0xf4, 0x75, 0x60, 0x65, 0x52, 0xb4, 0xfb, 0xbf, 0xac, 0x6e, 0xea, 0x5d,
    0xca, 0x0d, 0xb5, 0x66, 0xac, 0xba, 0x06, 0x30, 0x95, 0xf4, 0x96, 0x42, 0x7a, 0x7f, 0x58, 0x6d,
    0x83, 0x8e, 0xf6, 0x61, 0x7c, 0x0e, 0xfd, 0x09, 0x6e, 0x42, 0x6b, 0x1e, 0xb9, 0x14, 0x22, 0xf6,

    0x16, 0xd2, 0xd2, 0x60, 0x29, 0x23, 0x32, 0x9e, 0xb4, 0x82, 0xee, 0x58, 0x3a, 0x7d, 0x1f, 0x74,
    0x98, 0x5d, 0x17, 0x64, 0xe4, 0x6f, 0xf5, 0xad, 0x94, 0xaa, 0x89, 0xe3, 0xbe, 0x98, 0x91, 0x38,
    0x70, 0xec, 0x2f, 0x5e, 0x9f, 0xc9, 0xb1, 0x26, 0x3a, 0x64, 0x48, 0x13, 0xf1, 0x1a, 0xc5, 0xd5,
    0xe5, 0x66, 0x11, 0x11, 0x3a, 0xaa, 0x79, 0x45, 0x42, 0xb4, 0x57, 0x9d, 0x3f, 0xbc, 0xa3, 0xaa,
    0x98, 0x4e, 0x6b, 0x7a, 0x4a, 0x2f, 0x3e, 0x10, 0x7a, 0xc5, 0x33, 0x8d, 0xac, 0x0b, 0x79, 0x33,
    0x5d, 0x09, 0xfc, 0x9d, 0x9b, 0xe5, 0x18, 0xcd, 0x1c, 0x7c, 0x8b, 0x0a, 0xa8, 0x95, 0x56, 0xcc,
    0x4e, 0x34, 0x31, 0x33, 0xf5, 0xc1, 0xf5, 0x03, 0x0a, 0x4a, 0xb4, 0xd1, 0x90, 0xf1, 0x8f, 0x57,
    0x20, 0x05, 0x0d, 0xa0, 0xcd, 0x82, 0xb3, 0x25, 0xd8, 0xd2, 0x20, 0xf3, 0xc5, 0x96, 0x35, 0x35,
};


// key length is 8 bytes
// 加密概述: 密码类型: 流密码
// 采用密钥重叠循环，查表来进行异或。
//
void pp::pe::encode(unsigned char key[8], void *raw_data, size_t length) {
    unsigned char keys[8];
    memcpy(keys, key, 8);
    char *data = (char *)raw_data;
    for(int i = 0; i < length; i ++) {
        unsigned char n = ((keys[i & 7] + keys[(i + 1) & 7]) * keys[(i + 2) & 7] + keys[(i + 3) & 7]) & 0xff;
        data[i] ^= n ^ xor_table_[n];
        keys[i & 7] = (n * 2 + 3) & 0xff;

        if((i & 0xf) == 0) {
            key_random(key, keys, xor_table_[i & 0xff]);
        }
    }
}

// 解密
void pp::pe::decode(unsigned char key[8], void *raw_data, size_t length) {
    unsigned char keys[8];
    memcpy(keys, key, 8);
    char *data = (char *)raw_data;
    int j = 0;
    for(int i = 0; i < length; i ++) {
        unsigned char n = ((keys[i & 7] + keys[(i + 1) & 7]) * keys[(i + 2) & 7] + keys[(i + 3) & 7]) & 0xff;
        data[i] ^= n ^ xor_table_[n];
        keys[i & 7] = (n * 2 + 3) & 0xff;

        if((i & 0xf) == 0) {
            key_random(key, keys, xor_table_[i & 0xff]);
        }

    }
}

void pp::pe::key_random(unsigned char raw_key[8], unsigned char *out_key, unsigned char seed) {
    for(int i = 0; i < 8; i ++)  {
        out_key[i] = (raw_key[i] ^ xor_table_[raw_key[i]]) & 0xff;
        out_key[i] ^= seed + (unsigned char)i;
    }
}


/*
    pp 协议定义
*/
namespace pp {

enum class status {
    // Client request code
    connect = 0x10,
    data_transfer = 0x11,

    // Server response code
    protocol_error = 0x20,
    too_big = 0x21, // The content size too big
    invalid_transfer = 0x22, //
    ok = 0x30,
    send_key = 0x31,

    redirct = 0x40, // server set route
};

enum class data_type {
    binary_stream = 0x00,
    image = 0x01,
    video = 0x02,
    music = 0x03,

    text = 0x10,
    json = 0x11,
    xml  = 0x12,
};

// Size 32 bytes
struct header {
    unsigned short magic;    // "PP" 0x5050
    unsigned char status;    // The status code of client or server
    unsigned char type;      // The type of data
    unsigned int length;     // The length of data
    unsigned char route[6];           // The route of request
    unsigned char code[2];           // The code for idendtify client's data
};

}

namespace pp {
class pp_client {
public:
    pp_client() {}
    ~pp_client() {
        if(fd_ != -1) {
            ::close(fd_);
        }
    }
    bool connect(const std::string &ip, unsigned short port) {
        fd_ = socket(AF_INET, SOCK_STREAM, 0);
        bool ret = false;
        do {
            if(fd_ < 0) {
                perror("perror:");
                break;
            }

            struct sockaddr_in addr;
            bzero(&addr, sizeof(struct sockaddr));
            addr.sin_family = AF_INET;
            addr.sin_port = htons(port);
            addr.sin_addr.s_addr = inet_addr(ip.c_str());

            socklen_t address_len = sizeof(sockaddr);
            if(::connect(fd_, (struct sockaddr *)&addr, address_len) < 0) {
                perror("connect:");
                break;
            }

            pp::header header;
            memset(&header, 0, sizeof(pp::header));
            header.magic = 0x5050;
            header.status = (unsigned char)::pp::status::connect;
            header.type = (unsigned char)pp::data_type::binary_stream;
            write(fd_, &header, sizeof(pp::header));

            read(fd_, &header, sizeof(pp::header)); // read header
            if(header.magic != 0x5050) {
                std::cout << "connect error\n";
                break;
            }

            unsigned char key[8] = {0};
            pe().decode(key, &header.route, 8);
            memcpy(code_, header.code, 2);

            read(fd_, key_, 8); // read key
            pe().decode(key, key_, 8); // decrypt key

            ret = true;
        } while(false);
        is_connected_ = ret;

        return ret;
    }

    void send(char route[6], const std::string &data)  {
        if(is_connected_ == false) return;
        if(fd_ == -1) {
            return ;
        }
        pp::header header;
        memset(&header, 0, sizeof(pp::header));
        header.magic = 0x5050;
        header.status = (unsigned char)pp::status::data_transfer;
        header.type = (unsigned char)pp::data_type::binary_stream;
        header.length = htonl(data.size());

        memcpy(header.code, code_, 2);
        memcpy(header.route, route, 6);

        std::shared_ptr<vessel> send_buffer = std::shared_ptr<vessel>(new vessel);

        send_buffer->append(&header, sizeof(pp::header));
        *send_buffer << data;

        pe().encode(key_, send_buffer->data() + 8, 8);
        pe().encode(key_, send_buffer->data() + 16, send_buffer->size() - 8);

        while(send_buffer->size() > 0) {
            int write_len = write(fd_, send_buffer->data(), send_buffer->size());
            if(write_len < 0) {
                continue;
            }else if (write_len == 0) {
                break;
            }
            send_buffer->sub(write_len);
        }
    }
    std::shared_ptr<vessel> recv() {
        std::shared_ptr<vessel> recv_buffer = std::shared_ptr<vessel>(new vessel);
        if(is_connected_ == false) return recv_buffer;
        pp::header header;
        do {
            int read_len = -1;
            int readed_len = 0;
            while (read_len < 0) {
                read_len = read(fd_, &header, sizeof(pp::header)); // read header
                if(read_len == 0) {
                    std::cout << "disconnected!\n";
                    is_connected_ = false;
                    break;
                }
                else if(readed_len == sizeof(pp::header))
                    break;
                if(read_len > 0) readed_len += read_len;
            }

            if(!read_len) break;

            if(header.magic != 0x5050 && read_len != sizeof(pp::header)) {
                std::cout << "read error\n";
                break;
            }

            status_ = (pp::status)header.status;
            if(status_ == pp::status::invalid_transfer) {
                std::cout << "invalid_transfer\n";
            }

            pe().decode(key_, &header.route, 8);
            int length  = ntohl(header.length);

            readed_len = 0;
            while(recv_buffer->size() < length) {
                char buf[1024];
                int left = (length - recv_buffer->size()) % 1024;
                left = left ? left : 1024;

                int read_len = read(fd_, buf, left); // read key
                if(read_len < 0) continue;
                if(read_len == 0) {
                    std::cout << "disconnected\n";
                    is_connected_ = false;
                    break;
                }
                recv_buffer->append(buf, read_len);
                readed_len += read_len;
                //sleep(1);
            }
            pe().decode(key_, recv_buffer->data(), recv_buffer->size()); // decrypt key
        } while(false);
        return recv_buffer;
    }

    // 关闭socket
    void close() {
        ::close(fd_);
        fd_ = -1;
    }

private:
    int fd_ = -1;
    unsigned char key_[8] = {0};
    unsigned char code_[2] = {0};
    pp::status status_;
    bool is_connected_ = false;
};
}

